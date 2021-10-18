using System.Collections.Generic;
using System.Linq;

namespace RecShark.Extensions
{
    public static class PropertyExtensions
    {
        public static object GetPropertyValue<T>(this T element, string propertyName)
        {
            var propertyInfo = typeof(T).GetProperties().FirstOrDefault(p => p.Name == propertyName);
            return propertyInfo?.GetValue(element, null);
        }

        public static Dictionary<string, object> ToKeyValues<T>(this T element)
        {
            return typeof(T).GetProperties().ToDictionary(p => p.Name, p => p.GetValue(element, null));
        }

        public static string ToInfo<T>(this T element)
        {
            if (element.GetType().IsPrimitive)
                return element.ToString();

            var keyValues = element.ToKeyValues();
            var maxLength = keyValues.Keys.Max(k => k.Length);
            var format = $"{{0,{-maxLength}}} = '{{1}}'";

            return keyValues.Select(x => string.Format(format, x.Key, x.Value)).ToLines();
        }
    }
}