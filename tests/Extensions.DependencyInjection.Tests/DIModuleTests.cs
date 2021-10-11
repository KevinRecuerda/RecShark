using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.DependencyInjection.Tests
{
    public class DIModuleTests
    {
        [Fact]
        public void Should_be_equals_if_same_type()
        {
            var module1 = new DIModuleA();
            var module2 = new DIModuleA();

            module1.Should().Be(module2);
        }

        [Fact]
        public void Should_not_be_equals_if_not_same_type()
        {
            var module1 = new DIModuleA();
            var module2 = new DIModuleB();

            module1.Should().NotBe(module2);
        }
    }
}