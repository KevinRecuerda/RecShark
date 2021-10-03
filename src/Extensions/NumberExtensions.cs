using System;

namespace RecShark.Extensions
{
    public static class NumberExtensions
    {
        public static string Format(this double x, int digits = 2)
        {
            var delta = Math.Pow(10, -digits);
            if (Math.Abs(x).IsInRange(0, delta, RangeOptions.Excluded))
            {
                var direction = Math.Sign(x) >= 0 ? "< " : "> -";
                return $"{direction}{delta}";
            }

            return x.ToString($"N{digits}");
        }

        public static string Format(this double? x, int digits = 2)
        {
            return x.HasValue ? x.Value.Format(digits) : "";
        }

        public static double Ceiling(this double value, int digits = 2)
        {
            var multiplier = Math.Pow(10, digits);
            return Math.Ceiling(value * multiplier) / multiplier;
        }

        public static double Floor(this double value, int digits = 2)
        {
            var multiplier = Math.Pow(10, digits);
            return Math.Floor(value * multiplier) / multiplier;
        }
    }
}
