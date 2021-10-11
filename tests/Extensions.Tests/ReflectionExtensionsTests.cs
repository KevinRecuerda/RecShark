using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public class ReflectionExtensionsTests
    {
        private class Foo
        {
            public int AddFive(int number)
            {
                return number + 5;
            }
        }

        [Fact]
        public void InvokeMethodSafely__Should_invoke_method_with_args()
        {
            new Foo().InvokeMethodSafely("AddFive", 6).Should().Be(11);
        }

        [Fact]
        public void InvokeMethodSafely__Should_return_null_When_unknown_method()
        {
            new Foo().InvokeMethodSafely("AddSix", 6).Should().Be(null);
        }
    }
}