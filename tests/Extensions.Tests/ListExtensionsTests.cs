using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public class ListExtensionsTests
    {
        [Fact]
        public void Deconstruct__Should_deconstruct_1_item()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3, 4, 5 };

            // Act
            var (first, rest) = list;

            // Arrange
            first.Should().Be(1);
            rest.Should().ContainInOrder(2, 3, 4, 5);
        }

        [Fact]
        public void Deconstruct__Should_deconstruct_2_items()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3, 4, 5 };

            // Act
            var (first, second, rest) = list;

            // Arrange
            first.Should().Be(1);
            second.Should().Be(2);
            rest.Should().ContainInOrder(3, 4, 5);
        }

        [Fact]
        public void Deconstruct__Should_deconstruct_3_items()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3, 4, 5 };

            // Act
            var (first, second, third, rest) = list;

            // Arrange
            first.Should().Be(1);
            second.Should().Be(2);
            third.Should().Be(3);
            rest.Should().ContainInOrder(4, 5);
        }

        [Fact]
        public void Deconstruct__Should_manage_empty_list()
        {
            // Arrange
            var list = new List<int>();

            // Act
            var (first, rest) = list;

            // Arrange
            first.Should().Be(0);
            rest.Should().BeEmpty();
        }

        [Fact]
        public void Chunk__Should_chunk_list_by_size()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            const int chunkSize = 3;

            var expected = new List<List<int>>
            {
                new List<int> { 1, 2, 3 },
                new List<int> { 4, 5, 6 },
                new List<int> { 7, 8 }
            };

            // Act
            var actual = list.Chunk(chunkSize);

            // Arrange
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Chunk__Should_throw_exception_When_chunk_size_is_not_strictly_positive(int chunkSize)
        {
            // Arrange
            var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };

            // Act
            Action action = () => list.Chunk(chunkSize);

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("Chunk size must be strictly positive*");
        }

        [Fact]
        public void AddNotNull__Should_add_not_null_item()
        {
            // Arrange
            var list = new List<int> { 1, 2 };

            // Act
            list.AddNotNull(3);

            // Arrange
            list.Count.Should().Be(3);
            list.Last().Should().Be(3);
        }

        [Fact]
        public void AddNotNull__Should_not_add_null_item()
        {
            // Arrange
            var list = new List<string> { "a", "b" };

            // Act
            list.AddNotNull(null);

            // Arrange
            list.Count.Should().Be(2);
            list.Last().Should().Be("b");
        }
    }
}