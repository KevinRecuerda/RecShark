using System;
using System.Collections.Generic;

namespace RecShark.Extensions
{
    public static class ListExtensions
    {
        public static List<List<T>> Chunk<T>(this List<T> items, int chunkSize)
        {
            if (chunkSize < 1)
                throw new ArgumentException("Chunk size must be strictly positive", nameof(chunkSize));

            var result = new List<List<T>>();
            for (var i = 0; i < items.Count; i += chunkSize)
                result.Add(items.GetRange(i, Math.Min(chunkSize, items.Count - i)));
            return result;
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