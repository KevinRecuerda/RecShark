using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public class RangeExtensionsTests
    {
        public static IEnumerable<object[]> DateTimeData
        {
            get
            {
                var min = new DateTime(2000, 12, 01);
                var max = new DateTime(2000, 12, 31);

                return new[]
                {
                    new object[] { new DateTime(2000, 12, 15), min, max, true },
                    new object[] { min, min, max, true },
                    new object[] { max, min, max, true },
                    new object[] { max, max, max, true },
                    new object[] { new DateTime(2000, 11, 15), min, max, false },
                    new object[] { new DateTime(2001, 01, 15), min, max, false }
                };
            }
        }

        [Theory]
        [MemberData(nameof(DateTimeData))]
        public void IsInRange__Should_manage_datetime(DateTime item, DateTime min, DateTime max, bool expected)
        {
            var actual = item.IsInRange(min, max);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 0, 2, true)] // In
        [InlineData(1, 1, 2, true)] // = Min
        [InlineData(1, 0, 1, true)] // = Max
        [InlineData(1, 1, 1, true)] // = Min = Max
        [InlineData(1, 2, 4, false)] // < Min
        [InlineData(5, 2, 4, false)] // > Max
        public void IsInRange__Should_manage_int(int item, int min, int max, bool expected)
        {
            var actual = item.IsInRange(min, max);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 0, 2, true)] // In
        [InlineData(1, 1, 2, false)] // = Min
        [InlineData(1, 0, 1, false)] // = Max
        [InlineData(1, 1, 1, false)] // = Min = Max
        [InlineData(1, 2, 4, false)] // < Min
        [InlineData(5, 2, 4, false)] // > Max
        public void IsInRange__Should_exclude_range(int item, int min, int max, bool expected)
        {
            var actual = item.IsInRange(min, max, RangeOptions.Excluded);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 0, 2, true)] // In
        [InlineData(1, 1, 2, false)] // = Min
        [InlineData(1, 0, 1, true)] // = Max
        [InlineData(1, 1, 1, false)] // = Min = Max
        [InlineData(1, 2, 4, false)] // < Min
        [InlineData(5, 2, 4, false)] // > Max
        public void IsInRange__Should_exclude_min_only(int item, int min, int max, bool expected)
        {
            var actual = item.IsInRange(min, max, RangeOptions.MinExcluded);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 0, 2, true)] // In
        [InlineData(1, 1, 2, true)] // = Min
        [InlineData(1, 0, 1, false)] // = Max
        [InlineData(1, 1, 1, false)] // = Min = Max
        [InlineData(1, 2, 4, false)] // < Min
        [InlineData(5, 2, 4, false)] // > Max
        public void IsInRange__Should_exclude_max_only(int item, int min, int max, bool expected)
        {
            var actual = item.IsInRange(min, max, RangeOptions.MaxExcluded);

            actual.Should().Be(expected);
        }

        [Fact]
        public void In__Should_return_true_when_contained_by_items()
        {
            Gender.Male.In(Gender.Male).Should().BeTrue();
            Gender.Male.In(Gender.Female, Gender.Male).Should().BeTrue();
        }

        [Fact]
        public void In__Should_return_false_when_not_contained_by_items()
        {
            Gender.Male.In(Gender.Female).Should().BeFalse();
            Gender.Male.In().Should().BeFalse();
        }

        [Fact]
        public void In__Should_return_false_when_no_items()
        {
            Gender.Male.In().Should().BeFalse();
        }
    }

    public enum Gender
    {
        Male,
        Female
    }
}