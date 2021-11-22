using System;
using System.Collections.Generic;

namespace RecShark.Extensions
{
    using System.Linq;

    public static class DictionaryExtensions
    {
        /// <summary>
        /// return value if dict contains key
        /// otherwise return default value
        /// </summary>
        public static TValue GetSafely<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey                           key,
            TValue                         defaultValue = default)
        {
            return dictionary.TryGetValue(key, out var value)
                       ? value
                       : defaultValue;
        }

        /// <summary>
        /// return value if dict contains not null key
        /// otherwise return default value
        /// </summary>
        public static TValue GetSafely<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey?                          key,
            TValue                         defaultValue = default)
            where TKey : struct
        {
            return key.HasValue
                       ? dictionary.GetSafely(key.Value, defaultValue)
                       : defaultValue;
        }

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
        /// return converted value if dict contains not null key and can convert to required type
        /// otherwise return default value
        /// </summary>
        public static TReturn GetSafelyConverted<TKey, TValue, TReturn>(
            this IDictionary<TKey, TValue> dictionary,
            TKey?                          key,
            TReturn                        defaultValue = default)
            where TKey : struct
        {
            return key.HasValue
                       ? dictionary.GetSafelyConverted(key.Value, defaultValue)
                       : defaultValue;
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

        /// <summary>
        /// combine multiple dictionaries
        /// </summary>
        public static Dictionary<TKey, TValue> Combine<TKey, TValue>(this IEnumerable<Dictionary<TKey, TValue>> sources)
        {
            return sources.Flatten().ToDictionary();
        }

        /// <summary>
        /// create new dictionary from key-values
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> keyValues)
        {
            return keyValues.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
