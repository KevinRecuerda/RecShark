using System;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Testing.FluentAssertions.Tests
{
    public class FluentAssertionsExtensionsTests
    {
        private readonly ObjectForTests expected =  new ObjectForTests(1, 3.14, DateTime.Today);
        private readonly ObjectForTests actual   = new ObjectForTests(1, 3.15, DateTime.Today);
        
        [Fact]
        public void UsePrecision__Should_use_precision()
        {
            FluentAssertionsExtensions.UsePrecision(1E-2);
            actual.Should().BeEquivalentTo(expected);

            FluentAssertionsExtensions.UsePrecision(1E-3);
            actual.Should().NotBeEquivalentTo(expected);

            FluentAssertionsExtensions.UsePrecision(1E-2);
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void IsEquivalentTo__Should_return_equivalence()
        {
            FluentAssertionsExtensions.UsePrecision(1E-2);
            actual.IsEquivalentTo(expected).Should().Be(true);

            FluentAssertionsExtensions.UsePrecision(1E-3);
            actual.IsEquivalentTo(expected).Should().Be(false);
        }

        [Fact]
        public void IsEquivalentTo__Should_return_equivalence_with_options()
        {
            FluentAssertionsExtensions.UsePrecision(1E-3);
            actual.IsEquivalentTo(expected).Should().Be(false);

            actual.IsEquivalentTo(expected, options => options.Excluding(x => x.Value)).Should().Be(true);
        }

        [Fact]
        public void ExcludingSubCollectionMember__Should_ignore_sub_member_path()
        {
            new[] {actual}.Should().ContainEquivalentOf(expected, options => options.ExcludingSubCollectionMember("Value"));
        }
    }
}