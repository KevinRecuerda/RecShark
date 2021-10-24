using FluentAssertions;
using Xunit;

namespace RecShark.Algorithm.Tests
{
    public class SorterObjectTests
    {
        [Fact]
        public void TopologicalDepthSort__Should_correctly_sort_values_by_depth()
        {
            // Arrange
            var items = new[]
            {
                new Item("id1", "name"),
                new Item("id2", "name"),
                new Item("id3", "name")
            };
            var edges = new[]
            {
                (items[0], items[1]),
                (items[1], items[2])
            };

            // Act
            var actual = Sorter<Item>.TopologicalDepthSort(items, edges);

            // Assert
            actual.Count.Should().Be(3);
            actual[0].Should().BeEquivalentTo(items[2]);
            actual[1].Should().BeEquivalentTo(items[1]);
            actual[2].Should().BeEquivalentTo(items[0]);
        }

        private class Item
        {
            public Item(string id, string name)
            {
                Id = id;
                Name = name;
            }

            public string Id { get; set; }
            public string Name { get; set; }

            public override int GetHashCode()
            {
                return Id?.GetHashCode() ?? 0;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Item)obj);
            }

            public bool Equals(Item other)
            {
                return Id == other.Id;
            }
        }
    }
}