using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Togner.Common.ConsoleApp
{
    /// <summary>
    /// Arguments class, parses command line args.
    /// </summary>
    public sealed class Arguments
    {
        public const string NoValue = "@_NO_VALUE_ARGUMENT_@";
        private StringDictionary _parameters;

        /// <summary>
        /// Initializes a new instance of the Arguments class.
        /// </summary>
        /// <param name="args">String repre of the arguments.</param>
        public Arguments(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            this._parameters = new StringDictionary();
            var spliter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            string parameter = null;
            string[] parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'
            foreach (string txt in args)
            {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                parts = spliter.Split(txt, 3);
                switch (parts.Length)
                {
                    // Found a value (for the last parameter 
                    // found (space separator))
                    case 1:
                        if (parameter != null)
                        {
                            if (!this._parameters.ContainsKey(parameter))
                            {
                                parts[0] = remover.Replace(parts[0], "$1");
                                this._parameters.Add(parameter, parts[0]);
                            }
                            parameter = null;
                        }

                        // else Error: no parameter waiting for a value (skipped)
                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. 
                        // With no value, set it to Arguments.NoValue.
                        if (parameter != null)
                        {
                            if (!this._parameters.ContainsKey(parameter))
                            {
                                this._parameters.Add(parameter, Arguments.NoValue);
                            }
                        }
                        parameter = parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. 
                        // With no value, set it to Arguments.NoValue.
                        if (parameter != null)
                        {
                            if (!this._parameters.ContainsKey(parameter))
                            {
                                this._parameters.Add(parameter, Arguments.NoValue);
                            }
                        }
                        parameter = parts[1];

                        // Remove possible enclosing characters (",')
                        if (!this._parameters.ContainsKey(parameter))
                        {
                            parts[2] = remover.Replace(parts[2], "$1");
                            this._parameters.Add(parameter, parts[2]);
                        }
                        parameter = null;
                        break;
                }
            }

            // In case a parameter is still waiting
            if (parameter != null)
            {
                if (!this._parameters.ContainsKey(parameter))
                {
                    this._parameters.Add(parameter, Arguments.NoValue);
                }
            }
        }

        /// <summary>
        /// Retrieve a parameter value if it exists.
        /// </summary>
        /// <param name="parameter">Name of the param whose value we want.</param>
        /// <returns>Value of the param.</returns>
        public string this[string parameter]
        {
            get
            {
                return (this._parameters[parameter]);
            }
        }
    }
}
