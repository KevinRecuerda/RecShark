using System.ComponentModel;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public class EnumExtensionsTests
    {
        public enum TestEnum
        {
            [Description("EXPECTED")] AValue = 1,
            [Description("OTHER EXPECTED")] OtherValue = 2
        }

        [Fact]
        public void GetValues__Should_return_values()
        {
            EnumHelper<TestEnum>.GetValues().Should().BeEquivalentTo(new[] { TestEnum.AValue, TestEnum.OtherValue });
        }

        [Theory]
        [InlineData("AValue", TestEnum.AValue)]
        [InlineData("avalue", TestEnum.AValue)]
        [InlineData("AVALUE", TestEnum.AValue)]
        public void Parse__Should_parse_enum_value(string value, TestEnum expected)
        {
            EnumHelper<TestEnum>.Parse(value).Should().Be(expected);
        }

        [Theory]
        [InlineData("AValue", TestEnum.AValue)]
        [InlineData("avalue", TestEnum.AValue)]
        [InlineData("AVALUE", TestEnum.AValue)]
        [InlineData("Wrong", null)]
        [InlineData(null, null)]
        public void ParseSafely__Should_return_parse_enum_value(string value, TestEnum? expected)
        {
            EnumHelper<TestEnum>.ParseSafely(value).Should().Be(expected);
        }

        [Theory]
        [InlineData("EXPECTED", TestEnum.AValue)]
        [InlineData("OTHER EXPECTED", TestEnum.OtherValue)]
        [InlineData("UNKNOWN", null)]
        [InlineData(null, null)]
        public void ParseDescriptionSafely__Should_return_parse_enum_value(string value, TestEnum? expected)
        {
            EnumHelper<TestEnum>.ParseDescriptionSafely(value).Should().Be(expected);
        }

        [Theory]
        [InlineData(TestEnum.AValue, "EXPECTED")]
        [InlineData(TestEnum.OtherValue, "OTHER EXPECTED")]
        public void GetDescription__Should_return_enum_description(TestEnum value, string expected)
        {
            EnumHelper<TestEnum>.GetDescription(value).Should().Be(expected);
        }

        [Fact]
        public void GetDescriptions__Should_return_enum_descriptions_array()
        {
            EnumHelper<TestEnum>.GetDescriptions().Should().BeEquivalentTo(new[] { "EXPECTED", "OTHER EXPECTED" });
        }
    }
}