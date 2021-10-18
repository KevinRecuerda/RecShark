using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace RecShark.DependencyInjection.Tests
{
    public class DIModuleLoaderTests
    {
        [Fact]
        public void Load__Should_not_load_module_twice()
        {
            // Arrange
            var moduleA = new DIModuleA();
            var moduleB = new DIModuleB();
            var moduleC = new DIModuleC();

            // Act
            var services = new ServiceCollection();
            services.Load(moduleC, moduleB, moduleA);

            // Assert
            moduleC.CallCount.Should().Be(1);
            moduleB.CallCount.Should().Be(1);
            moduleA.CallCount.Should().Be(1);
        }

        [Fact]
        public void Load__Should_load_dependencies_first()
        {
            // Act
            var services = new ServiceCollection();
            services.Load<DIModuleA>();

            // Assert
            services.Count.Should().Be(1);
            services[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
        }
    }
}