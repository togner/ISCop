using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISCop.Rules;
using log4net;
using Microsoft.SqlServer.Dts.Runtime;
using StyleCop;
using Togner.Common.ConsoleApp;

namespace ISCop
{
    /// <summary>
    /// TODO: 
    /// Fix literals (make const, quote args), make source more uniform (path to component?), move Result creating code to base class
    /// Design/Perf
    ///     Visit each component once 
    ///     Run all the rules based on component type
    /// Place rules in separate assembly, reflection + attribs + config file
    /// </summary>
    public static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "ISCop.Program.PrintHelp(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ispac")]
        public static int Main(string[] args)
        {
            int exitCode = 0;
            try
            {
                Program.Logger.Info("Args: " + string.Join("\n", args));

                // params
                var arguments = new Arguments(args);
                if (arguments["?"] != null)
                {
                    Program.PrintHelp(string.Empty);
                    return 1;
                }
                var ispacPath = arguments["ispac"];
                if (string.IsNullOrEmpty(ispacPath)
                    || ispacPath.Equals(Arguments.NoValue)
                    || !File.Exists(ispacPath))
                {
                    Program.PrintHelp(string.Format(CultureInfo.CurrentCulture, "Invalid path to .ispac file: '{0}'", ispacPath));
                    return 1;
                }
                else
                {
                    var styleCopSettingsPath = arguments["scop"];
                    if (string.IsNullOrEmpty(styleCopSettingsPath)
                        || styleCopSettingsPath.Equals(Arguments.NoValue)
                        || !File.Exists(styleCopSettingsPath))
                    {
                        styleCopSettingsPath = Path.GetFullPath("Settings.StyleCop");
                    }
                    var packageName = arguments["pkg"];
                    if (!string.IsNullOrEmpty(packageName) 
                        && packageName.Equals(Arguments.NoValue))
                    {
                        packageName = null;
                    }

                    // A: Open project as ispac (needs to be built)
                    // B: Create new project, load proj param, cm from xml files - https://social.msdn.microsoft.com/Forums/sqlserver/en-US/ff62aafa-3b19-46e3-839e-8353bf4ab6df/problem-loading-ssis-project-parameters-out-of-projectparams-file?forum=sqlintegrationservices
                    // Load dtsx - does it get the project configs (cm, parameters) automagically?
                    foreach (var result in Program.Analyze(ispacPath, styleCopSettingsPath, packageName))
                    {
                        result.Log(Program.Logger);
                    }
                }
            }
            catch (Exception exception)
            {
                Program.Logger.Error("Exception caught in Main()", exception);
                exitCode = 2;
            }
            return exitCode;
        }

        private static IEnumerable<Result> Analyze(string ispacPath, string styleCopSettingsPath, string packageName)
        {
            using (var proj = Project.OpenProject(ispacPath))
            {
                foreach (var pkgItem in proj.PackageItems.OrderBy(p => p.StreamName))
                {
                    if (!string.IsNullOrEmpty(packageName) 
                        && !pkgItem.StreamName.Equals(packageName + ".dtsx", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    var pkg = pkgItem.Package;

                    // Validation errors and warnings
                    // E.g. references to non-existing package connection managers.
                    pkg.Validate(null, null, null, null);
                    foreach (var error in pkg.Errors)
                    {
                        yield return new Result(error, pkg.Name);
                    }
                    foreach (var warning in pkg.Warnings)
                    {
                        yield return new Result(warning, pkg.Name);
                    }

                    // Custom rules
                    foreach (var rule in new PackageRule[]
                    {
                        new ExecuteProcessTaskLogging(),
                        new DataflowScriptStyleCop(styleCopSettingsPath),
                        new DataflowScriptCSharp(),
                        new DataflowCount(),
                        new DataflowAsynchronousPaths(),
                        new DataflowAccessMode(),
                        new DataflowSortTransformations(),
                        new PackageProtectionLevel(),
                        new VariableEvaluateAsExpression(),
                        new TaskProperties(),
                        new ScriptTaskCSharp(),
                        new ScriptTaskStyleCop(styleCopSettingsPath)
                    }.OrderBy(r => r.Id))
                    {
                        rule.Check(pkg);
                        foreach (var result in rule.Results)
                        {
                            yield return result;
                        }
                    }
                }
            }
        }

        private static void PrintHelp(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Program.Logger.Info(message);
            }
            Program.Logger.Info(
@"Usage:
    ISCop.exe -ispac:<path to .ispac file> [-scop:<path to Settings.StyleCop>] [-pkg:<package name>] 
");
        }
    }
}
