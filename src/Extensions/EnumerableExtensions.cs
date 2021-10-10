using System;
using System.Collections.Generic;
using System.Linq;

namespace RecShark.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source?.Any() != true;
        }

        public static string ToString<T>(this IEnumerable<T> items, string separator)
        {
            return string.Join(separator ?? Environment.NewLine, items);
        }

        public static string ToLines<T>(this IEnumerable<T> items)
        {
            return items.ToString(Environment.NewLine);
        }

        public static bool ContainsAny<T>(this IEnumerable<T> list, params T[] items)
        {
            return list.Intersect(items).Any();
        }

        public static IEnumerable<TSource> ConcatIf<TSource>(this IEnumerable<TSource> source, TSource element, bool condition)
        {
            if (condition)
                source = source.Concat(new[] {element});
            return source;
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.Aggregate(Enumerable.Empty<T>(), (all, x) => all.Concat(x));
        }

        public static TSource MaxOrDefault<TSource>(
            this IEnumerable<TSource> source,
            TSource defaultValue = default)
        {
            return source.MaxOrDefault(x => x, defaultValue);
        }

        public static TResult MaxOrDefault<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            TResult defaultValue = default)
        {
            return CompareDefault(source, selector, 1, defaultValue);
        }

        public static TSource? MaxOrNull<TSource>(this IEnumerable<TSource> source)
            where TSource : struct, IComparable
        {
            return source.MaxOrNull(x => x);
        }

        public static TResult? MaxOrNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            where TResult : struct, IComparable
        {
            return CompareNull(source, selector, 1);
        }

        public static TSource MinOrDefault<TSource>(
            this IEnumerable<TSource> source,
            TSource defaultValue = default)
        {
            return source.MinOrDefault(x => x, defaultValue);
        }

        public static TResult MinOrDefault<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            TResult defaultValue = default)
        {
            return CompareDefault(source, selector, -1, defaultValue);
        }

        public static TSource? MinOrNull<TSource>(this IEnumerable<TSource> source)
            where TSource : struct, IComparable
        {
            return source.MinOrNull(x => x);
        }

        public static TResult? MinOrNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            where TResult : struct, IComparable
        {
            return CompareNull(source, selector, -1);
        }

        private static TResult CompareDefault<TSource, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            int comparison,
            TResult defaultValue = default)
        {
            var (itemFound, best) = Compare(source, selector, comparison);

            return itemFound ? best : defaultValue;
        }

        private static TResult? CompareNull<TSource, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            int comparison)
            where TResult : struct, IComparable
        {
            var (itemFound, best) = Compare(source, selector, comparison);

            return itemFound ? best : (TResult?) null;
        }

        private static (bool, TResult) Compare<TSource, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            int comparison)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var comparer = Comparer<TResult>.Default;
            var best = default(TResult);
            var itemFound = false;

            foreach (var item in source)
            {
                var value = selector(item);
                if (!itemFound)
                {
                    best = value;
                    itemFound = true;
                }
                else
                {
                    if (comparer.Compare(value, best) == comparison)
                        best = value;
                }
            }

            return (itemFound, best);
        }
    }
}