using System;
using System.Linq;

namespace RecShark.Extensions
{
    public static class RangeExtensions
    {
        public static bool IsInRange<T>(this T item, T min, T max, RangeOptions options = RangeOptions.Included)
            where T : IComparable<T>
        {
            var left  = item.CompareTo(min); 
            var right = item.CompareTo(max);

            var minIncluded = ((int) options&1) == 0;
            var maxIncluded = ((int) options&2) == 0;

            return (left  > 0 || minIncluded && left  == 0)
                && (right < 0 || maxIncluded && right == 0);
        }

        public static bool IsIn<T>(this T item, params T[] items)
        {
            return items?.Contains(item) ?? false;
        }
    }

    public enum RangeOptions
    {
        Included    = 0,
        MinExcluded = 1,
        MaxExcluded = 2,
        Excluded    = 3
    }
}