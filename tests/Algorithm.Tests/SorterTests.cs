using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace RecShark.Algorithm.Tests
{
    public class SorterTests
    {
        [Fact]
        public void TopologicalDepthSort__Should_correctly_sort_values_by_depth()
        {
            // Arrange
            var items = new[] { 1, 2, 3 };
            var edges = new[]
            {
                (1, 2),
                (2, 3)
            };

            // Act
            var actual = Sorter<int>.TopologicalDepthSort(items, edges);

            // Assert
            actual.Count.Should().Be(3);
            actual[0].Should().BeEquivalentTo(3);
            actual[1].Should().BeEquivalentTo(2);
            actual[2].Should().BeEquivalentTo(1);
        }

        [Fact]
        public void TopologicalDepthSort__Should_manage_multiple_branches()
        {
            // Arrange
            var items = new[] { 1, 2, 3, 4 };
            var edges = new[]
            {
                (1, 2),
                (3, 4)
            };

            // Act
            var actual = Sorter<int>.TopologicalDepthSort(items, edges);

            // Assert
            actual.Count.Should().Be(2);
            actual[0].Should().BeEquivalentTo(2, 4);
            actual[1].Should().BeEquivalentTo(1, 3);
        }

        [Fact]
        public void TopologicalDepthSort__Should_throw_cyclic_dependency_exception_When_value_depends_on_itself()
        {
            // Arrange
            var items = new[] { 1, 2, 3 };
            var edges = new[] { (1, 1) };

            // Act
            Func<Dictionary<int, List<int>>> action = () => Sorter<int>.TopologicalDepthSort(items, edges);

            // Assert
            var exception = action.Should().Throw<CyclicDependencyException<int>>();
            exception.And.Nodes.Should().HaveCount(1);
        }

        [Fact]
        public void TopologicalDepthSort__Should_throw_cyclic_dependency_exception_When_direct_cyclic_dependency()
        {
            // Arrange
            var items = new[] { 1, 2, 3 };
            var edges = new[]
            {
                (1, 2),
                (2, 1)
            };

            // Act
            Func<Dictionary<int, List<int>>> action = () => Sorter<int>.TopologicalDepthSort(items, edges);

            // Assert
            var exception = action.Should().Throw<CyclicDependencyException<int>>();
            exception.And.Nodes.Should().HaveCount(2);
        }

        [Fact]
        public void TopologicalDepthSort__Should_throw_cyclic_dependency_exception_When_indirect_cyclic_dependency()
        {
            // Arrange
            var items = new[] { 1, 2, 3 };
            var edges = new[]
            {
                (1, 2),
                (2, 3),
                (3, 1)
            };

            // Act
            Func<Dictionary<int, List<int>>> action = () => Sorter<int>.TopologicalDepthSort(items, edges);

            // Assert
            var exception = action.Should().Throw<CyclicDependencyException<int>>();
            exception.And.Nodes.Should().HaveCount(3);
        }
    }
}