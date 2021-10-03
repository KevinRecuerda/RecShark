using System;
using System.Collections.Generic;

namespace RecShark.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// return converted value if dict contains key and can convert to required type
        /// otherwise return default value
        /// </summary>
        public static TReturn GetSafelyConverted<TKey, TValue, TReturn>(
            this IDictionary<TKey, TValue> dictionary,
            TKey                           key,
            TReturn                        defaultValue = default)
        {
            if (dictionary.TryGetValue(key, out var value) && value.TryConvertTo(out TReturn result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// set value if dict don't contain key
        /// otherwise do nothing
        /// </summary>
        public static bool SetSafely<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey                           key,
            TValue                         value)
        {
            if (dictionary.ContainsKey(key))
                return false;

            dictionary[key] = value;
            return true;
        }

        /// <summary>
        /// transform source to dictionary, ignoring duplicate keys (keep first or last).
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionarySafely<T, TKey, TValue>(
            this IEnumerable<T> source,
            Func<T, TKey>       keySelector,
            Func<T, TValue>     valueSelector,
            bool                keepFirst = true)
        {
            var dictionary = new Dictionary<TKey, TValue>();
            foreach (var item in source)
            {
                var key = keySelector(item);
                if (keepFirst && dictionary.ContainsKey(key))
                    continue;

                var value = valueSelector(item);
                dictionary[key] = value;
            }

            return dictionary;
        }

        /// <summary>
        /// apply all key/values from other to source
        /// </summary>
        public static Dictionary<TKey, TValue> Apply<TKey, TValue>(
            this Dictionary<TKey, TValue> source,
            Dictionary<TKey, TValue>      other)
        {
            foreach (var (key, value) in other)
                source[key] = value;

            return source;
        }
    }
}