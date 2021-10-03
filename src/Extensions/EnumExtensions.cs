using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace RecShark.Extensions
{
    public static class EnumExtensions
    {
        public static T? ToEnum<T>(this string enumName)
            where T : struct, IConvertible
        {
            return EnumHelper<T>.ParseSafely(enumName);
        }

        public static string GetDescription<T>(this T? enumValue)
            where T : struct, IConvertible
        {
            return enumValue?.GetDescription();
        }

        public static string GetDescription<T>(this T enumValue)
            where T : struct, IConvertible
        {
            return EnumHelper<T>.GetDescription(enumValue);
        }
    }

    public static class EnumHelper<T>
        where T : struct, IConvertible
    {
        public static T[] GetValues()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }

        public static T? ParseSafely(string enumName)
        {
            if (Enum.TryParse(enumName, true, out T @enum))
                return @enum;

            return null;
        }

        public static T Parse(string enumName)
        {
            return (T) Enum.Parse(typeof(T), enumName, true);
        }

        public static T? ParseDescriptionSafely(string enumDescription)
        {
            if (enumDescription == null)
                return null;

            enumDescription = enumDescription.Trim();

            foreach (var field in typeof(T).GetFields())
            {
                var description = GetDescription(field);
                if (description == enumDescription)
                    return (T) field.GetValue(null);
            }

            return null;
        }

        public static string GetDescription(T value)
        {
            var text = value.ToString(CultureInfo.InvariantCulture);
            var info = value.GetType().GetField(text);

            var description = GetDescription(info);
            return description ?? text;
        }

        public static string[] GetDescriptions()
        {
            var descriptions = GetValues().Select(GetDescription).ToArray();
            return descriptions;
        }

        private static string GetDescription(MemberInfo info)
        {
            var attribute = (DescriptionAttribute) Attribute.GetCustomAttribute(info, typeof(DescriptionAttribute));
            return attribute?.Description;
        }
    }
}