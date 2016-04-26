using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using log4net;

namespace Togner.Common.Tools
{
    public static class GenericTools
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GenericTools));

        /// <summary>
        /// Converts non-nullable type (bool, int, DateTime, etc.) to nullable type (bool?, int?).
        /// Handles nulls and DBNulls, may not be bulletproof.
        /// </summary>
        /// <typeparam name="T">Type of struct to convert.</typeparam>
        /// <param name="value">The struct to convert.</param>
        /// <returns>The struct converted to nullable type.</returns>
        public static Nullable<T> ConvertToNullable<T>(object value) where T : struct
        {
            Nullable<T> convertedValue = new Nullable<T>();
            try
            {
                if (value != DBNull.Value && value != null)
                {
                    convertedValue = (T)value;
                }
            }
            catch (InvalidCastException castException)
            {
                GenericTools.Logger.Error(MethodInfo.GetCurrentMethod(), castException);
            }
            return convertedValue;
        }

        /// <summary>
        /// Convert an object into a specified type using a default value to fall back to if the value is null, DBNull or cannot be converted.
        /// </summary>
        /// <typeparam name="T"> Type to convert to.</typeparam>
        /// <param name="value"> The object to convert.</param>
        /// <param name="defaultValue"> The default value to use if the value is null or DBNull or is not of the proper type.</param>
        /// <returns> The value converted to the T type.</returns>
        public static T ConvertToWithFallback<T>(object value, T defaultValue)
        {
            T result = defaultValue;
            try
            {
                Type t = typeof(T);
                if (value != DBNull.Value && value != null)
                {
                    if (t.IsEnum)
                    {
                        result = (T)value;
                    }
                    else
                    {
                        result = (T)Convert.ChangeType(value, typeof(T), CultureInfo.CurrentCulture);
                    }
                }
            }
            catch (InvalidCastException castException)
            {
                GenericTools.Logger.Error(MethodInfo.GetCurrentMethod(), castException);
            }
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        public static bool SetIfNotNullOrDbNull<T>(ref T valueToSet, object value)
        {
            if (value == null || Convert.IsDBNull(value))
            {
                return false;
            }
            valueToSet = GenericTools.ConvertToWithFallback<T>(value, default(T));
            return true;
        }

        /// <summary>
        /// String.Contains that is not case sensitive.
        /// </summary>
        /// <param name="first">The input string.</param>
        /// <param name="second">The string to look for.</param>
        /// <returns>True if the value parameter occurs within this string, or if value is the empty string (""); otherwise, false.</returns>
        public static bool ContainsIgnoreCase(this string first, string second)
        {
            if (string.IsNullOrEmpty(first))
            {
                return string.IsNullOrEmpty(second);
            }
            else if (string.IsNullOrEmpty(second))
            {
                return false;
            }
            return first.ToUpperInvariant().Contains(second.ToUpperInvariant());
        }

        /// <summary>
        /// Appends the string representation of a specified object to the end of this instance. 
        /// Prepends separator if this is not the first string in this instance.
        /// </summary>
        /// <param name="builder">A reference to this instance.</param>
        /// <param name="value">The object to append.</param>
        /// <param name="separator">The separator to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public static StringBuilder Append(this StringBuilder builder, object value, object separator)
        {
            if (builder == null)
            {
                return builder;
            }
            if (builder.Length != 0)
            {
                builder.Append(separator);
            }
            return builder.Append(value);
        }

        /// <summary>
        /// Math ceiling to significant digits.
        /// </summary>
        /// <param name="value">The number to round up.</param>
        /// <param name="significantFigures">The significant figures in the resulting number (1-14).</param>
        /// <returns>New double rounded up on success; 0, NaN, +inf, -inf are not rounded up.</returns>
        public static double CeilingSignificantDigits(double value, int significantFigures)
        {
            if (value == 0.0d)
            {
                return value;
            }
            else if (double.IsNaN(value))
            {
                return double.NaN;
            }
            else if (double.IsPositiveInfinity(value))
            {
                return double.PositiveInfinity;
            }
            else if (double.IsNegativeInfinity(value))
            {
                return double.NegativeInfinity;
            }
            else if (significantFigures < 1 || significantFigures > 14)
            {
                throw new ArgumentOutOfRangeException("significantFigures", value, "The significantFigures argument must be between 0 and 15 exclusive.");
            }

            /*
             * Math.Round rounds to integers, so we have to "normalize" the number 
             * (based on how many sigfigures we have) => e.g. 34121.56565 (sigFig := 1) => 0.3
             * or 0.0034 (sigfig := 1) => 0.3
             * -scale
             * --ceiling(log10(abs(value))) to get number of digits of the scale
             * -round 
             * -scale back and take the upper bound
             */
            double scale = Math.Pow(10, Math.Ceiling(Math.Log10(Math.Abs(value * Math.Pow(10, -significantFigures)))));
            double rounded = Math.Round(value / scale, significantFigures, MidpointRounding.AwayFromZero);

            return Math.Ceiling(rounded) * scale;
        }
    }
}
