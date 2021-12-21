using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace RecShark.DependencyInjection.Tests
{
    public class DependencyInjectionExtensionsTests
    {
        [Fact]
        public void AddSingleton__Should_add_same_instance()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddSingleton<ISample, IOther, Sample>();

            // Assert
            services.Count.Should().Be(3);

            var provider = services.BuildServiceProvider();
            var sample   = provider.GetService<Sample>();
            var iSample  = provider.GetService<ISample>();
            var other    = provider.GetService<IOther>();

            sample.Should().Be(iSample);
            sample.Should().Be(other);
        }

        [Fact]
        public void Remove__Should_remove_all_registered_description()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<ISample, Sample>();
            services.AddTransient<ISample, Sample>();

            // Act
            services.Remove<ISample>();

            // Assert
            services.Count.Should().Be(0);
        }

        [Fact]
        public void Reset__Should_remove_all_registered_description_and_add_override()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<ISample, Sample>();

            var @new = ServiceDescriptor.Singleton<ISample, Sample>();

            // Act
            services.Reset(@new);

            // Assert
            services.Count.Should().Be(1);
            services.Contains(@new).Should().Be(true);
        }

        [Fact]
        public void RemoveImpl__Should_remove_all_impl_mappings()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ISample, IOther, Sample>();

            // Act
            services.RemoveImpl<Sample>();

            // Assert
            services.Count.Should().Be(0);
        }
    }
}
