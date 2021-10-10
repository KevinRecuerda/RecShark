using System;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public class DateTimeExtensionsTests : Testing.Tests
    {
        [Theory]
        [InlineData("2020-01-30", null, null, true)]
        [InlineData("2020-01-30", null, "2020-01-31", true)]
        [InlineData("2020-01-30", "2020-01-29", null, true)]
        [InlineData("2020-01-30", "2020-01-30", "2020-01-30", true)]
        [InlineData("2020-01-30", "2020-01-29", "2020-01-31", true)]
        [InlineData("2020-01-30", "2020-01-31", null, false)]
        [InlineData("2020-01-30", "2020-01-28", "2020-01-29", false)]
        [InlineData("2020-01-30", null, "2020-01-29", false)]
        public void IsBetween__Should_return_correct_value(DateTime date, string startIso, string endIso, bool expected)
        {
            var start = startIso != null ? DateTime.Parse(startIso) : (DateTime?) null;
            var end = endIso != null ? DateTime.Parse(endIso) : (DateTime?) null;

            var actual = date.IsBetween(start, end);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("2019-01-01", true)]
        [InlineData("2019-01-02", true)]
        [InlineData("2019-01-03", true)]
        [InlineData("2019-01-04", true)]
        [InlineData("2019-01-05", false)]
        [InlineData("2019-01-06", false)]
        [InlineData("2019-01-07", true)]
        [InlineData("2019-01-08", true)]
        public void IsWeekDay__Should_return_correct_value(DateTime date, bool expected)
        {
            date.IsWeekDay().Should().Be(expected);
        }

        [Theory]
        [InlineData("2019-01-11", false)] // Friday
        [InlineData("2019-01-12", true)]
        [InlineData("2019-01-13", true)]
        [InlineData("2019-01-14", false)]
        [InlineData("2019-01-15", false)]
        [InlineData("2019-01-16", false)]
        [InlineData("2019-01-17", false)]
        [InlineData("2019-01-18", false)] // Friday
        [InlineData("2019-01-19", true)]
        public void IsWeekend__Should_return_correct_value(DateTime date, bool expected)
        {
            date.IsWeekend().Should().Be(expected);
        }

        [Theory]
        [InlineData("2018-06-18", false)]
        [InlineData("2018-06-19", false)]
        [InlineData("2018-06-20", false)]
        [InlineData("2018-06-21", false)]
        [InlineData("2018-06-22", true)]
        [InlineData("2018-06-23", false)]
        [InlineData("2018-06-24", false)]
        public void IsLastWeekDay__Should_return_correct_value(DateTime date, bool expected)
        {
            date.IsLastWeekDay().Should().Be(expected);
        }

        [Theory]
        [InlineData("2019-01-11", "2019-01-11")] // Friday
        [InlineData("2019-01-12", "2019-01-18")]
        [InlineData("2019-01-13", "2019-01-18")]
        [InlineData("2019-01-14", "2019-01-18")]
        [InlineData("2019-01-15", "2019-01-18")]
        [InlineData("2019-01-16", "2019-01-18")]
        [InlineData("2019-01-17", "2019-01-18")]
        [InlineData("2019-01-18", "2019-01-18")] // Friday
        [InlineData("2019-01-19", "2019-01-25")]
        public void FirstDay__Should_return_current_or_next_friday(DateTime date, DateTime expected)
        {
            date.FirstDay(DayOfWeek.Friday).Should().Be(expected);
        }

        [Theory]
        [InlineData("2019-01-11", "2019-01-10")] // Friday -> Thursday
        [InlineData("2019-01-12", "2019-01-11")] // Saturday -> Friday
        [InlineData("2019-01-13", "2019-01-11")] // Sunday -> Friday
        [InlineData("2019-01-14", "2019-01-11")] // Monday -> Friday
        [InlineData("2019-01-15", "2019-01-14")] // Tuesday -> Monday
        public void PreviousWeekDay__Should_return_correct_value(DateTime date, DateTime expected)
        {
            date.PreviousWeekDay().Should().Be(expected);
        }

        [Theory]
        [InlineData("2019-01-05", 1, "2019-01-04")] // Saturday -> Friday
        [InlineData("2019-01-06", 1, "2019-01-04")] // Sunday -> Friday
        [InlineData("2019-01-07", 1, "2019-01-04")]
        [InlineData("2019-01-08", 1, "2019-01-07")]
        [InlineData("2019-01-09", 1, "2019-01-08")]
        [InlineData("2019-01-10", 1, "2019-01-09")]
        [InlineData("2019-01-11", 1, "2019-01-10")]
        [InlineData("2019-01-05", 2, "2019-01-03")] // Saturday -> Thursday
        [InlineData("2019-01-06", 2, "2019-01-03")] // Sunday -> Thursday
        [InlineData("2019-01-07", 2, "2019-01-03")] // Monday -> Thursday
        [InlineData("2019-01-08", 2, "2019-01-04")] // Tuesday -> Friday
        [InlineData("2019-01-09", 2, "2019-01-07")]
        [InlineData("2019-01-10", 2, "2019-01-08")]
        [InlineData("2019-01-11", 2, "2019-01-09")]
        public void WeekDayEarlier__Should_return_correct_value(DateTime date, int n, DateTime expected)
        {
            date.WeekDayEarlier(n).Should().Be(expected);
        }

        [Theory]
        [InlineData("2019-01-01", 1, "2018-12-25")]
        [InlineData("2019-01-02", 2, "2018-12-19")]
        [InlineData("2019-01-07", 3, "2018-12-17")]
        public void WeekEarlier__Should_return_correct_value(DateTime date, int n, DateTime expected)
        {
            date.WeekEarlier(n).Should().Be(expected);
        }

        [Theory]
        [InlineData("2019-01-11", 1, "2018-12-11")] // January -> December
        [InlineData("2019-6-11", 5, "2019-01-11")] // June -> January
        public void MonthEarlier__Should_return_correct_value(DateTime date, int n, DateTime expected)
        {
            date.MonthEarlier(n).Should().Be(expected);
        }

        [Theory]
        [InlineData("2019-01-01", "2019-01-31")]
        [InlineData("2019-02-01", "2019-02-28")]
        [InlineData("2019-03-01", "2019-03-29")]
        public void LastWeekDayOfMonth__Should_return_correct_value(DateTime date, DateTime expected)
        {
            date.LastWeekDayOfMonth().Should().Be(expected);
        }

        [Theory]
        [InlineData("2019-01-01", "2019-03-29")] // 03/29 Q1 Friday
        [InlineData("2019-02-15", "2019-03-29")]
        [InlineData("2019-03-31", "2019-03-29")]
        [InlineData("2019-04-01", "2019-06-28")] // 06/28 Q2 Friday
        [InlineData("2019-05-02", "2019-06-28")]
        [InlineData("2019-06-01", "2019-06-28")]
        [InlineData("2019-08-01", "2019-09-30")] // 09/30 Q3 Monday
        [InlineData("2019-12-31", "2019-12-31")] // 12/31 Q4 Tuesday
        public void LastWeekDayOfQuarter__Should_return_correct_value(DateTime date, DateTime expected)
        {
            date.LastWeekDayOfQuarter().Should().Be(expected);
        }

        [Theory]
        [InlineData("2019-01-01", "2019-06-28")]
        [InlineData("2019-02-15", "2019-06-28")]
        [InlineData("2019-03-31", "2019-06-28")]
        [InlineData("2019-06-01", "2019-06-28")]
        [InlineData("2019-08-01", "2019-12-31")]
        [InlineData("2019-12-31", "2019-12-31")]
        public void LastWeekDayOfBiannual__Should_return_correct_value(DateTime date, DateTime expected)
        {
            date.LastWeekDayOfBiannual().Should().Be(expected);
        }

        [Fact]
        public void ToIso__Should_display_date_as_iso()
        {
            var dateTime = new DateTime(2017, 04, 13, 14, 47, 18, 531);
            dateTime.ToIso().Should().Be("2017-04-13");
        }

        [Fact]
        public void ToIso__Should_display_empty_string_When_date_is_null()
        {
            ((DateTime?) null).ToIso().Should().Be("");
        }

        [Fact]
        public void ToIso__Should_display_date_as_iso_when_date_has_value()
        {
            var dateTime = new DateTime(2017, 04, 13, 14, 47, 18, 531);
            ((DateTime?) dateTime).ToIso().Should().Be("2017-04-13");
        }

        [Fact]
        public void ToDate__Should_parse_iso_format()
        {
            "2019-01-01".ToDate().Should().Be(new DateTime(2019, 01, 01));
        }

        [Fact]
        public void ToDateSafely__Should_parse_iso_format()
        {
            "2019-01-01".ToDateSafely().Should().Be(new DateTime(2019, 01, 01));
        }

        [Fact]
        public void ToDateSafely__Should_return_null_when_no_iso_format()
        {
            "20190101".ToDateSafely().Should().Be(null);
        }

        [Fact]
        public void ToDateSafely__Should_return_default_when_no_iso_format()
        {
            var defaultValue = new DateTime(2019, 12, 31);
            "20190101".ToDateSafely(defaultValue: defaultValue).Should().Be(defaultValue);
        }

        [Fact]
        public void RemoveMilliseconds__Should_remove_millisecond_from_date()
        {
            var dateTime = new DateTime(2017, 04, 13, 14, 47, 18, 531);
            dateTime.RemoveMilliseconds().Should().Be(dateTime.AddMilliseconds(-dateTime.Millisecond));
        }

        private static readonly Func<DateTime, DateTime> RangeSplitter = d => d.AddDays(2);

        [Fact]
        public void Split__Should_not_split_range_When_shorter_than_max_size()
        {
            // Arrange
            var from = new DateTime(2000, 01, 01);
            var to = new DateTime(2000, 01, 02);

            // Act
            var actual = DateTimeExtensions.Split(from, to, RangeSplitter);

            // Assert
            actual.Should().BeEquivalentTo((from, to));
        }

        [Fact]
        public void Split__Should_not_split_range_When_equal_to_max_size()
        {
            // Arrange
            var from = new DateTime(2000, 01, 01);
            var to = new DateTime(2000, 01, 03);

            // Act
            var actual = DateTimeExtensions.Split(from, to, RangeSplitter);

            // Assert
            actual.Should().BeEquivalentTo((from, to));
        }

        [Theory]
        [InlineData("2000-01-04")]
        [InlineData("2000-01-05")]
        [InlineData("2000-01-06")]
        public void Split__Should_split_range_When_longer_than_max_size(DateTime to)
        {
            // Arrange
            var from = new DateTime(2000, 01, 01);

            // Act
            var actual = DateTimeExtensions.Split(from, to, RangeSplitter);

            // Assert
            var expected = new[]
            {
                (from, new DateTime(2000, 01, 03)),
                (new DateTime(2000, 01, 04), to)
            };
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("2000-01-07")]
        [InlineData("2000-01-08")]
        [InlineData("2000-01-09")]
        public void Split__Should_split_range_in_more_than_two_ranges(DateTime to)
        {
            // Arrange
            var from = new DateTime(2000, 01, 01);

            // Act
            var actual = DateTimeExtensions.Split(from, to, RangeSplitter);

            // Assert
            var expected = new[]
            {
                (from, new DateTime(2000, 01, 03)),
                (new DateTime(2000, 01, 04), new DateTime(2000, 01, 06)),
                (new DateTime(2000, 01, 07), to)
            };
            actual.Should().BeEquivalentTo(expected);
        }
    }
}