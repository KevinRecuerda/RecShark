using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public partial class EnumerableExtensionsTests
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

        [Fact]
        public void IntersectAll__Should_intersect_with_all_lists()
        {
            // Arrange
            var lists = new[]
            {
                Enumerable.Range(0, 3),
                Enumerable.Range(1, 4),
                Enumerable.Range(1, 5),
                Enumerable.Range(0, 6)
            };

            // Act
            var actual = lists.IntersectAll();

            // Assert
            actual.Should().BeEquivalentTo(Enumerable.Range(1, 2));
        }

        [Fact]
        public void Partition__Should_partition_list_according_to_conditions()
        {
            // Arrange
            var items = new[] {-15, -10, -5, 0, 5, 10, 15};

            // Act
            var actual = items.Partition(
                x => x > 10,
                x => x >= 0,
                x => x <= -10);

            // Assert
            actual.Should().HaveCount(4);
            actual[0].Should().ContainInOrder(15);
            actual[1].Should().ContainInOrder(0, 5, 10);
            actual[2].Should().ContainInOrder(-15, -10);
            actual[3].Should().ContainInOrder(-5);
        }

        [Fact]
        public void Partition__Should_manage_no_conditions()
        {
            // Arrange
            var items = new[] {-15, -10, -5, 0, 5, 10, 15};

            // Act
            var actual = items.Partition();

            // Assert
            actual.Should().HaveCount(1);
            actual[0].Should().BeEquivalentTo(items);
        }
    }
}
