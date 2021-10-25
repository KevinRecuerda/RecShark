using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public class ConverterExtensionsTests : Testing.Tests
    {
        [Theory]
        [InlineData("a word", default(double))]
        [InlineData(null, default(double))]
        [InlineData(12, 12)]
        [InlineData("12.123", 12.123)]
        public void ConvertSafelyTo__Should_convert_safe_to_double(object value, double expected)
        {
            value.ConvertSafelyTo<double>().Should().Be(expected);
        }

        [Fact]
        public void ConvertSafelyTo__Should_fallback_on_default_value()
        {
            "a word".ConvertSafelyTo(1.5).Should().Be(1.5);
        }

        [Theory]
        [InlineData("a word", "a word", true)]
        [InlineData(null, null, true)]
        [InlineData(12, "12", true)]
        public void TryConvertTo__Should_try_convert_string(object value, string expectedResult, bool expected)
        {
            value.TryConvertTo<string>(out var result).Should().Be(expected);
            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("a word", default(double), false)]
        [InlineData(null, default(double), false)]
        [InlineData(12, 12, true)]
        [InlineData("12.123", 12.123, true)]
        public void TryConvertTo__Should_try_convert_double(object value, double expectedResult, bool expected)
        {
            value.TryConvertTo<double>(out var result).Should().Be(expected);
            result.Should().Be(expectedResult);
        }
    }
}