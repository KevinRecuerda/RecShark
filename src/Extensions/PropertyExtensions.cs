namespace RecShark.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
            var format    = $"{{0,{-maxLength}}} = '{{1}}'";

            return keyValues.Select(x => string.Format(format, x.Key, x.Value)).ToLines();
        }

        public static PropertyAccessor<T, TResult> ToAccessor<T, TResult>(this T item, Func<T, TResult> selector)
        {
            return new PropertyAccessor<T, TResult>(item, selector);
        }

        public class PropertyAccessor<T, TResult>
        {
            public T                Item     { get; }
            public Func<T, TResult> Selector { get; }

            public PropertyAccessor(T item, Func<T, TResult> selector)
            {
                this.Item     = item;
                this.Selector = selector;
            }

            public TResult Get()
            {
                return this.Selector(this.Item);
            }

            public override string ToString()
            {
                return this.Selector(this.Item).ToString();
            }
        }
    }
}