using System;
using System.Reflection;

namespace Togner.Common.Tools
{
    /// <summary>
    /// Extension functions for enum.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Will get the string value for a given enums value, this will
        /// only work if you assign the StringEnumAttribute to
        /// the items in your enum.
        /// </summary>
        /// <param name="value">The enum value to get the string value from.</param>
        /// <returns>The string representing the enum value.</returns>
        public static string GetStringValue(this Enum value)
        {
            if (value == null)
            {
                return null;
            }
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
            string stringValue = null;
            if (attribs != null && attribs.Length > 0)
            {
                stringValue = attribs[0].Value;
            }
            return stringValue;
        }
    }

    /// <summary>
    /// This attribute is used to represent a string value
    /// for a value in an enum.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class StringValueAttribute : Attribute 
    {
        /// <summary>
        /// Gets the string value for a value in an enum.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the StringValueAttribute class.
        /// </summary>
        /// <param name="value">String value this attribute is holding.</param>
        public StringValueAttribute(string value) 
        {
            this.Value = value;
        }
    }
}
