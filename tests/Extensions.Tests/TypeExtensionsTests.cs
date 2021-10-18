using System;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public class TypeExtensionsTests
    {
        private class Foo
        {
        }

        private enum Bar
        {
        }

        [Theory]
        [InlineData(typeof(Foo))]
        [InlineData(typeof(string))]
        public void IsTypeNullable__Should_return_true_When_reference_type(Type type)
        {
            type.IsNullableType().Should().Be(true);
        }

        [Theory]
        [InlineData(typeof(int?))]
        [InlineData(typeof(DateTime?))]
        [InlineData(typeof(Bar?))]
        [InlineData(typeof(char?))]
        public void IsTypeNullable__Should_return_true_When_nullable_value_type(Type type)
        {
            type.IsNullableType().Should().Be(true);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Bar))]
        [InlineData(typeof(char))]
        public void IsTypeNullable__Should_return_false_When_value_type(Type type)
        {
            type.IsNullableType().Should().Be(false);
        }
    }
}