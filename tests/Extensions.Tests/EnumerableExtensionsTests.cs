using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void IsNullOrEmpty__Should_return_true_When_list_is_null()
        {
            ((IEnumerable<string>) null).IsNullOrEmpty().Should().Be(true);
        }

        [Fact]
        public void IsNullOrEmpty__Should_return_true_When_list_is_empty()
        {
            new List<string>().IsNullOrEmpty().Should().Be(true);
        }

        [Fact]
        public void IsNullOrEmpty__Should_return_false_When_list_contains_values()
        {
            Enumerable.Range(0, 2).IsNullOrEmpty().Should().Be(false);
        }

        [Fact]
        public void ToString__Should_join_list()
        {
            Enumerable.Range(0, 2).ToString(",").Should().Be("0,1");
        }

        [Fact]
        public void ToLines__Should_join_list_using_lines()
        {
            Enumerable.Range(0, 2).ToLines().Should().Be($"0{Environment.NewLine}1");
        }

        [Fact]
        public void ContainsAny__Should_return_true_When_intersection_is_not_empty()
        {
            Enumerable.Range(0, 2).ContainsAny(1, 2).Should().Be(true);
        }

        [Fact]
        public void ContainsAny__Should_return_false_When_intersection_is_empty()
        {
            Enumerable.Range(0, 2).ContainsAny(5, 6).Should().Be(false);
        }

        [Fact]
        public void ConcatIf__Should_concat_if_condition_is_true()
        {
            var actual = Enumerable.Range(0, 2).ConcatIf(2, true);
            actual.Count().Should().Be(3);
        }

        [Fact]
        public void ConcatIf__Should_ignore_if_condition_is_false()
        {
            var actual = Enumerable.Range(0, 2).ConcatIf(2, false);
            actual.Count().Should().Be(2);
        }

        [Fact]
        public void Flatten__Should_flatten_multiple_lists()
        {
            // Arrange
            var lists = new[]
            {
                Enumerable.Range(0, 2),
                Enumerable.Range(2, 2)
            };

            // Act & Assert
            lists.Flatten().Should().BeEquivalentTo(Enumerable.Range(0, 4));
        }

        [Fact]
        public void Flatten__Should_return_empty_When_no_list()
        {
            // Arrange
            var lists = new IEnumerable<int>[0];

            // Act & Assert
            lists.Flatten().Should().BeEmpty();
        }

        [Theory]
        [InlineData(false, -1, 9)]
        [InlineData(true, -1, -1)]
        public void MaxOrDefault__Should_return_max(bool isEmpty, int defaultValue, int expected)
        {
            var list = isEmpty ? Enumerable.Empty<int>() : Enumerable.Range(0, 10);
            list.MaxOrDefault(defaultValue).Should().Be(expected);
        }

        [Theory]
        [InlineData(false, -1, 9)]
        [InlineData(true, -1, -1)]
        public void MaxOrDefault__Should_return_max_with_selector(bool isEmpty, int defaultValue, int expected)
        {
            var list = isEmpty ? Enumerable.Empty<int>() : Enumerable.Range(0, 10);
            list.MaxOrDefault(x => x, defaultValue).Should().Be(expected);
        }

        [Theory]
        [InlineData(false, 9)]
        [InlineData(true, null)]
        public void MaxOrNull__Should_return_max(bool isEmpty, int? expected)
        {
            var list = isEmpty ? Enumerable.Empty<int>() : Enumerable.Range(0, 10);
            list.MaxOrNull().Should().Be(expected);
        }

        [Theory]
        [InlineData(false, 9)]
        [InlineData(true, null)]
        public void MaxOrNull__Should_return_max_with_selector(bool isEmpty, int? expected)
        {
            var list = isEmpty ? Enumerable.Empty<int>() : Enumerable.Range(0, 10);
            list.MaxOrNull(x => x).Should().Be(expected);
        }

        [Theory]
        [InlineData(false, -1, 10)]
        [InlineData(true, -1, -1)]
        public void MinOrDefault__Should_return_min(bool isEmpty, int defaultValue, int expected)
        {
            var list = isEmpty ? Enumerable.Empty<int>() : Enumerable.Range(10, 10);
            list.MinOrDefault(defaultValue).Should().Be(expected);
        }

        [Theory]
        [InlineData(false, -1, 10)]
        [InlineData(true, -1, -1)]
        public void MinOrDefault__Should_return_min_with_selector(bool isEmpty, int defaultValue, int expected)
        {
            var list = isEmpty ? Enumerable.Empty<int>() : Enumerable.Range(10, 10);
            list.MinOrDefault(x => x, defaultValue).Should().Be(expected);
        }

        [Theory]
        [InlineData(false, 10)]
        [InlineData(true, null)]
        public void MinOrNull__Should_return_min(bool isEmpty, int? expected)
        {
            var list = isEmpty ? Enumerable.Empty<int>() : Enumerable.Range(10, 10);
            list.MinOrNull().Should().Be(expected);
        }

        [Theory]
        [InlineData(false, 10)]
        [InlineData(true, null)]
        public void MinOrNull__Should_return_min_with_selector(bool isEmpty, int? expected)
        {
            var list = isEmpty ? Enumerable.Empty<int>() : Enumerable.Range(10, 10);
            list.MinOrNull(x => x).Should().Be(expected);
        }
    }
}