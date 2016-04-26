using System.Globalization;
using log4net;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop
{
    public enum ResultType
    {
        Error,
        Warning,
        Information
    }

    public class Result
    {
        public ResultType Severity { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Package { get; set; }
        public string Source { get; set; }
        public int Line { get; set; }

        public Result(ResultType severity, string id, string name, string description, string package, string source, int line)
        {
            this.Severity = severity;
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Package = package;
            this.Source = source;
            this.Line = line;
        }

        public Result(DtsError error, string packageName)
            : this(ResultType.Error, 
            error != null ? error.ErrorCode.ToString(CultureInfo.CurrentCulture) : null, 
            null,
            error != null ? error.Description : null, 
            packageName, 
            error != null ? error.Source : null, 
            -1)
        {
        }

        public Result(DtsWarning warning, string packageName)
            : this(ResultType.Warning,
            warning != null ? warning.WarningCode.ToString(CultureInfo.CurrentCulture) : null, 
            null, 
            warning != null ? warning.Description : null, 
            packageName,
            warning != null ? warning.Source : null, 
            -1)
        {
        }

        public string ToString(bool includeSeverity)
        {
            var result = string.Format(CultureInfo.CurrentCulture, "{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                this.Id,
                this.Name,
                this.Description.Replace("\r", string.Empty).Replace("\n", string.Empty),
                this.Package,
                this.Source != this.Package ? this.Source : null,
                this.Line != -1 ? (object)this.Line : null);
            if (includeSeverity)
            {
                result = this.Severity.ToString().Substring(0, 4).ToUpperInvariant() + "\t" + result;
            }
            return result;
        }

        public override string ToString()
        {
            return this.ToString(true);
        }

        public void Log(ILog logger)
        {
            if (logger != null)
            {
                switch (this.Severity)
                {
                    case ResultType.Error:
                        logger.Error(this.ToString(false));
                        break;
                    case ResultType.Warning:
                        logger.Warn(this.ToString(false));
                        break;
                    case ResultType.Information:
                        logger.Info(this.ToString(false));
                        break;
                }
            }
        }
    }
}
