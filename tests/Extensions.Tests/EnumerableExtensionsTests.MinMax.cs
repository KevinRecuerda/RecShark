using System.Linq;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public partial class EnumerableExtensionsTests
    {
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