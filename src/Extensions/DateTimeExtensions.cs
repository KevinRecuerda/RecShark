using System;
using System.Collections.Generic;
using System.Globalization;
using FluentDate;
using FluentDateTime;

namespace RecShark.Extensions
{
    // CHECK: PR on FluentDataTime
    public static class DateTimeExtensions
    {
        public const string IsoFormat = "yyyy-MM-dd";

        public static bool IsBetween(this DateTime date, DateTime? start, DateTime? end)
        {
            return (start ?? DateTime.MinValue) <= date
                   && (end ?? DateTime.MaxValue) >= date;
        }

        public static bool IsWeekend(this DateTime date)
        {
            return date.DayOfWeek.In(DayOfWeek.Saturday, DayOfWeek.Sunday);
        }

        public static bool IsWeekDay(this DateTime date)
        {
            return !date.IsWeekend();
        }

        public static bool IsLastWeekDay(this DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Friday;
        }

        public static DateTime LastWeekDay(this DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Sunday => date.AddDays(-2),
                DayOfWeek.Saturday => date.AddDays(-1),
                _ => date
            };
        }

        public static DateTime PreviousWeekDay(this DateTime date)
        {
            return date.PreviousDay().LastWeekDay();
        }

        public static DateTime FirstDay(this DateTime date, DayOfWeek day)
        {
            while (date.DayOfWeek != day)
                date = date.NextDay();

            return date;
        }

        public static DateTime WeekDayEarlier(this DateTime date, int n)
        {
            for (var i = 0; i < n; i++)
                date = date.PreviousWeekDay();

            return date;
        }

        public static DateTime WeekEarlier(this DateTime date, int n = 1)
        {
            return date - n.Weeks();
        }

        public static DateTime MonthEarlier(this DateTime date, int n = 1)
        {
            return date.AddMonths(-n);
        }

        public static DateTime LastWeekDayOfMonth(this DateTime date)
        {
            return date.LastDayOfMonth().LastWeekDay();
        }

        public static DateTime LastWeekDayOfQuarter(this DateTime date)
        {
            return date.LastDayOfQuarter().LastWeekDay();
        }

        public static DateTime LastWeekDayOfBiannual(this DateTime date)
        {
            var month = date.Month > 6 ? 12 : 6;
            var firstDayOf = date.SetDate(date.Year, month, 1);
            return firstDayOf.LastWeekDayOfMonth();
        }

        public static DateTime ToDate(this string str, string format = IsoFormat)
        {
            return DateTime.ParseExact(str, format, CultureInfo.InvariantCulture);
        }

        public static DateTime? ToDateSafely(
            this string str,
            string format = IsoFormat,
            DateTime? defaultValue = null)
        {
            return DateTime.TryParseExact(str, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date
                : defaultValue;
        }

        public static string ToIso(this DateTime dateTime)
        {
            return dateTime.ToString(IsoFormat);
        }

        public static string ToIso(this DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToIso() : "";
        }

        public static DateTime RemoveMilliseconds(this DateTime dateTime)
        {
            return dateTime.AddTicks(-1L * dateTime.Ticks % 10000000L);
        }

        public static IEnumerable<(DateTime @from, DateTime to)> Split(DateTime @from, DateTime to, Func<DateTime, DateTime> rangeSplitter)
        {
            var periods = new List<(DateTime from, DateTime to)>();

            var rangeFrom = @from;
            var rangeTo = rangeSplitter(rangeFrom);
            while (rangeTo < to)
            {
                periods.Add((rangeFrom, rangeTo));
                rangeFrom = rangeTo.AddDays(1);
                rangeTo = rangeSplitter(rangeFrom);
            }

            periods.Add((rangeFrom, to));
            return periods;
        }
    }
}