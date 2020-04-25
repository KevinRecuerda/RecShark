using System;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Testing.FluentAssertions.Tests
{
    public class ContainExtensionsTests
    {
        private readonly ObjectForTests obj1 = new ObjectForTests(1, 3.14, DateTime.Today);
        private readonly ObjectForTests obj2 = new ObjectForTests(2, 3.15, DateTime.Today);
        private readonly ObjectForTests obj3 = new ObjectForTests(3, 3.16, DateTime.Today);
        
        [Fact]
        public void ContainEquivalentOf__Should_contain_item()
        {
            var items = new[] {obj1, obj2, obj3};
            items.Should().ContainEquivalentOf(obj1, obj2);
        }
        
        [Fact]
        public void ContainEquivalentOf__Should_contain_multiple_items()
        {
            var items = new[] {obj1, obj2, obj3};
            items.Should().ContainEquivalentOf(obj1, obj2);
        }

        [Fact]
        public void ContainEquivalentOf__Should_throw_exception_when_item_is_not_contained()
        {
            var    items  = new[] {obj1};
            Action action = () => items.Should().ContainEquivalentOf(obj1, obj2, obj3);

            action.Should().Throw<Exception>()
                  .WithMessage("Expected collection * to contain equivalent of * Id = 2*");
        }

        [Fact]
        public void NotContainEquivalentOf__Should_not_contain_item()
        {
            var actual = new[] {obj1, obj2};
            actual.Should().NotContainEquivalentOf(obj3);
        }

        [Fact]
        public void NotContainEquivalentOf__Should_not_contain_multiple_items()
        {
            var actual = new[] {obj1};
            actual.Should().NotContainEquivalentOf(obj2, obj3);
        }

        [Fact]
        public void NotContainEquivalentOf__Should_throw_exception_when_item_is_contained()
        {
            var    items  = new[] {obj1, obj2, obj3};
            Action action = () => items.Should().NotContainEquivalentOf(obj1, obj2);

            action.Should().Throw<Exception>()
                  .WithMessage("Expected collection * to not contain equivalent of * Id = 1*");
        }
    }
}