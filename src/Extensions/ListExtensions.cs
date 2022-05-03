using System.Collections.Generic;
using System.Linq;

namespace RecShark.Extensions
{
    public static class ListExtensions
    {
        // credit: https://stackoverflow.com/a/47816647
        public static void Deconstruct<T>(this IList<T> list, out T first, out IList<T> rest)
        {
            first = list.Count > 0 ? list[0] : default;
            rest  = list.Skip(1).ToList();
        }

        public static void Deconstruct<T>(this IList<T> list, out T first, out T second, out IList<T> rest)
        {
            first  = list.Count > 0 ? list[0] : default;
            second = list.Count > 1 ? list[1] : default;
            rest   = list.Skip(2).ToList();
        }

        public static void Deconstruct<T>(this IList<T> list, out T first, out T second, out T third, out IList<T> rest)
        {
            first  = list.Count > 0 ? list[0] : default;
            second = list.Count > 1 ? list[1] : default;
            third  = list.Count > 2 ? list[2] : default;
            rest   = list.Skip(3).ToList();
        }

        public static List<T> InList<T>(this T item)
        {
            return new List<T> {item};
        }

        public static void AddNotNull<T>(this List<T> list, T item)
        {
            if (item != null)
                list.Add(item);
        }
    }
}
