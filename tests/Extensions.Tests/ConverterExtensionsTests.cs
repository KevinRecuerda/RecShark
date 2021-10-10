using FluentAssertions;
using Xunit;

 namespace RecShark.Extensions.Tests
{
    public class ConverterExtensionsTests : Testing.Tests
    {
        [Theory]
        [InlineData("a word", "a word", true)]
        [InlineData(null,     null,     true)]
        [InlineData(12,       "12",     true)]
        public void TryConvertTo__Should_try_convert_string(object value, string expectedResult, bool expected)
        {
            value.TryConvertTo<string>(out var result).Should().Be(expected);
            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("a word", default(double), false)]
        [InlineData(null,     default(double), false)]
        [InlineData(12,       12,              true)]
        [InlineData("12.123", 12.123,          true)]
        public void TryConvertTo__Should_try_convert_double(object value, double expectedResult, bool expected)
        {
            value.TryConvertTo<double>(out var result).Should().Be(expected);
            result.Should().Be(expectedResult);
        }
    }
}