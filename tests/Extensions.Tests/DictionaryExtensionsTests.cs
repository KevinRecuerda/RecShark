using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public class DictionaryExtensionsTests : Testing.Tests
    {
        [Fact]
        public void GetSafelyConverted__Should_return_safe_result_converted()
        {
            // Arrange
            var source = new Dictionary<string, string>()
            {
                ["KEY1"] = "A word",
                ["KEY2"] = "123.456"
            };

            // Act & Assert
            source.GetSafelyConverted("KEY1", default(string)).Should().Be("A word");
            source.GetSafelyConverted("KEY2", default(string)).Should().Be("123.456");
            source.GetSafelyConverted("KEY1", default(double)).Should().Be(0);
            source.GetSafelyConverted("KEY2", default(double)).Should().Be(123.456);
            source.GetSafelyConverted("KEY2", default(int)).Should().Be(0);
        }

        [Fact]
        public void SetSafely__Should_set_value_if_key_not_exists()
        {
            // Arrange
            var source = new Dictionary<string, string>()
            {
                ["KEY1"] = "A word",
                ["KEY2"] = "123.456"
            };

            // Act & Assert
            source.SetSafely("KEY3", "test").Should().Be(true);
            source["KEY3"].Should().Be("test");
        }

        [Fact]
        public void SetSafely__Should_not_set_value_if_key_already_exists()
        {
            // Arrange
            var source = new Dictionary<string, string>()
            {
                ["KEY1"] = "A word",
                ["KEY2"] = "123.456",
                ["KEY3"] = "already exists"
            };

            // Act & Assert
            source.SetSafely("KEY3", "test").Should().Be(false);
            source["KEY3"].Should().Be("already exists");
        }

        [Fact]
        public void ToDictionarySafely__Should_ignore_duplicates()
        {
            // Arrange
            var listWithDuplicates = new[]
            {
                ("KEY1", "VALUE1"),
                ("KEY2", "VALUE2"),
                ("KEY3", "VALUE3"),
                ("KEY2", "another2"),
                ("KEY1", "another1"),
            };

            var expected = new Dictionary<string, string>()
            {
                ["KEY1"] = "VALUE1",
                ["KEY2"] = "VALUE2",
                ["KEY3"] = "VALUE3"
            };

            // Act
            var actual = listWithDuplicates.ToDictionarySafely(x => x.Item1, x => x.Item2);

            // Arrange
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Apply__Should_apply_other_dictionary_to_source()
        {
            // Arrange
            var source = new Dictionary<string, string>()
            {
                ["KEY1"] = "A",
                ["LEFT_KEY"] = "123"
            };
            var other = new Dictionary<string, string>()
            {
                ["KEY1"] = "B",
                ["RIGHT_KEY"] = "456"
            };

            var expected = new Dictionary<string, string>()
            {
                ["KEY1"] = "B",
                ["LEFT_KEY"] = "123",
                ["RIGHT_KEY"] = "456"
            };

            // Act
            var actual = source.Apply(other);

            // Arrange
            actual.Should().BeEquivalentTo(expected);
        }
    }
}