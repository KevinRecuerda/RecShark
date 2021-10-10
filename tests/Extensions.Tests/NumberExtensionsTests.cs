using FluentAssertions;
using Xunit;

 namespace RecShark.Extensions.Tests
{
    public class NumberExtensionsTests : Testing.Tests
    {
        [Theory]
        [InlineData(0,         "0.00")]
        [InlineData(1,         "1.00")]
        [InlineData(100.5,     "100.50")]       // keep only 2 digits
        [InlineData(100.591,   "100.59")]       // keep only 2 digits
        [InlineData(1234567.8, "1,234,567.80")] // keep only 2 digits
        [InlineData(0.003,     "< 0.01")]       // positive small
        [InlineData(-0.003,    "> -0.01")]      // negative small
        public void Format__Should_display_double(double number, string expected)
        {
            number.Format().Should().Be(expected);
        }

        [Fact]
        public void Format__Should_return_empty_string_When_double_is_null()
        {
            ((double?) null).Format().Should().Be("");
        }

        [Theory]
        [InlineData(20.00002,  0,  21)]
        [InlineData(20.00002,  1,  20.1)]
        [InlineData(20.00002,  2,  20.01)]
        [InlineData(20.00002,  3,  20.001)]
        [InlineData(20.00002,  6,  20.00002)]
        [InlineData(-20.00002, 15, -20.00002)]
        public void Ceiling__Should_return_ceiling_number(double actual, int digits, double expected)
        {
            actual.Ceiling(digits).Should().Be(expected);
        }

        [Theory]
        [InlineData(20.000002,  0,  20)]
        [InlineData(20.000002,  2,  20)]
        [InlineData(-20.00002,  0, -21)]
        [InlineData(20.999999,  3, 20.999)]
        public void Floor__Should_return_floor_number(double actual, int digits, double expected)
        {
            actual.Floor(digits).Should().Be(expected);
        }
    }
}