﻿using System;
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

        private enum Bar { }

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