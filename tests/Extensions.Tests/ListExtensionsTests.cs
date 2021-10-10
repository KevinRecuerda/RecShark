﻿using System;
 using System.Collections.Generic;
 using System.Linq;
 using FluentAssertions;
 using Xunit;

 namespace RecShark.Extensions.Tests
{
    public class ListExtensionsTests
    {
        [Fact]
        public void Chunk__Should_chunk_list_by_size()
        {
            // Arrange
            var       list      = new List<int> {1, 2, 3, 4, 5, 6, 7, 8};
            const int chunkSize = 3;

            var expected = new List<List<int>>
            {
                new List<int> {1, 2, 3},
                new List<int> {4, 5, 6},
                new List<int> {7, 8}
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
            var list = new List<int> {1, 2, 3, 4, 5, 6, 7, 8};

            // Act
            Action action = () => list.Chunk(chunkSize);

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("Chunk size must be strictly positive*");
        }

        [Fact]
        public void AddNotNull__Should_add_not_null_item()
        {
            // Arrange
            var list = new List<int> {1, 2};

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
            var list = new List<string> {"a", "b"};

            // Act
            list.AddNotNull(null);

            // Arrange
            list.Count.Should().Be(2);
            list.Last().Should().Be("b");
        }
    }
}