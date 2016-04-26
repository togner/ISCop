﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISCop.Rules;
using log4net;
using Microsoft.SqlServer.Dts.Runtime;
using Togner.Common.ConsoleApp;

namespace ISCop
{
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

                var ispacPath = arguments["i"];

                if (string.IsNullOrEmpty(ispacPath)
                    || ispacPath.Equals(Arguments.NoValue)
                    || !File.Exists(ispacPath))
                {
                    Program.PrintHelp(string.Format(CultureInfo.CurrentCulture, "Invalid path to .ispac file: '{0}'", ispacPath));
                    return 1;
                }
                else
                {
                   // A: Open project as ispac (needs to be built)
                   // B: Create new project, load proj param, cm from xml files - https://social.msdn.microsoft.com/Forums/sqlserver/en-US/ff62aafa-3b19-46e3-839e-8353bf4ab6df/problem-loading-ssis-project-parameters-out-of-projectparams-file?forum=sqlintegrationservices
                   // Load dtsx - does it get the project configs (cm, parameters) automagically?
                   foreach (var result in Program.Analyze(ispacPath))
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

        private static IEnumerable<Result> Analyze(string ispacPath)
        {
            using (var proj = Project.OpenProject(ispacPath))
            {
                foreach (var pkgItem in proj.PackageItems)
                {
                    var pkg = pkgItem.Package;
                    Program.Logger.InfoFormat("Analyzing {0}...", pkg.Name);

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
                        new DataFlowCount(),
                        new DataFlowAsynchronousPaths(),
                        new AccessMode(),
                        new DataFlowSortTransformations()
                    })
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
    ISCop.exe -i:<path to .ispac file>
");
        }
    }
}
