using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecShark.Extensions
{
    public static partial class EnumerableExtensions
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

        // credit: https://stackoverflow.com/a/1674779
        public static IEnumerable<T> IntersectAll<T>(this IEnumerable<IEnumerable<T>> lists)
        {
            HashSet<T> hashSet = null;
            foreach (var list in lists)
            {
                if (hashSet == null)
                {
                    hashSet = new HashSet<T>(list);
                }
                else
                {
                    hashSet.IntersectWith(list);
                }
            }

            return hashSet == null ? new List<T>() : hashSet.ToList();
        }

        public static List<T>[] Partition<T>(this IEnumerable<T> list, params Func<T, bool>[] conditions)
        {
            if (conditions == null || conditions.Length == 0)
                return new[] {list.ToList()};

            var subLists = Enumerable.Range(0, conditions.Length + 1).Select(i => new List<T>()).ToArray();
            foreach (var item in list)
            {
                var i = MatchCondition(item, conditions);
                subLists[i].Add(item);
            }

            return subLists;
        }

        public static List<T>[] Split<T>(this IEnumerable<T> items, int size)
        {
            if (size < 1)
                throw new ArgumentException("Size must be strictly positive", nameof(size));

            return items.Select((item, i) => new {item, i})
                        .GroupBy(x => x.i % size, x => x.item)
                        .Select(g => g.ToList())
                        .ToArray();
        }

        public static List<T>[] Chunk<T>(this IEnumerable<T> items, int chunkSize)
        {
            if (chunkSize < 1)
                throw new ArgumentException("Chunk size must be strictly positive", nameof(chunkSize));

            return items.Select((item, i) => new {item, i})
                        .GroupBy(x => x.i / chunkSize, x => x.item)
                        .Select(g => g.ToList())
                        .ToArray();
        }

        private static int MatchCondition<T>(T item, Func<T, bool>[] conditions)
        {
            for (var i = 0; i < conditions.Length; i++)
            {
                if (conditions[i](item))
                    return i;
            }

            return conditions.Length;
        }
    }
}
