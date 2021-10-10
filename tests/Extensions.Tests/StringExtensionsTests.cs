using System;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public class StringExtensionsTests : Testing.Tests
    {
        [Theory]
        [InlineData("hello world!")]
        [InlineData("hello*")]
        [InlineData("*world*")]
        [InlineData("*!")]
        [InlineData("*")]
        [InlineData("hello", "hello*")]
        public void SmartMatchAny__Should_match(params string[] patterns)
        {
            const string text = "hello world!";
            text.SmartMatchAny(patterns).Should().Be(true);
        }

        [Theory]
        [InlineData("hello")]
        [InlineData("*world")]
        [InlineData("!")]
        [InlineData("people")]
        [InlineData("")]
        public void SmartMatchAny__Should_not_match(params string[] patterns)
        {
            const string text = "hello world!";
            text.SmartMatchAny(patterns).Should().Be(false);
        }
        
        [Fact]
        public void SmartMatchAny__Should_not_match_When_no_patterns()
        {
            const string text = "hello world!";
            text.SmartMatchAny().Should().Be(false);
        }

        [Theory]
        [InlineData(1, 4)]
        [InlineData(2, 7)]
        [InlineData(3, -1)]
        public void IndexOfN__Should_return_index_of_char(int n, int expected)
        {
            const string text = "hello world";

            var actual = text.IndexOfN('o', n);

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void IndexOfN__Should_throw_exception_When_n_is_not_positive(int n)
        {
            Action action = () => "test".IndexOfN('e', n);

            action.Should().Throw<ArgumentException>().WithMessage("n should be > 0*");
        }

        [Fact]
        public void Prefixing__Should_return_text_with_prefix()
        {
            "hello".Prefixing("[tag]").Should().Be("[tag] hello");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Prefixing__Should_ignore_null_or_empty(string value)
        {
            "hello".Prefixing(value).Should().Be("hello");
        }

        [Fact]
        public void Tag__Should_tag_one_element()
        {
            "hello".Tag("TECH").Should().Be("[TECH] hello");
        }

        [Fact]
        public void Tag__Should_tag_multiple_elements()
        {
            "hello".Tag("PEOPLE").Tag("TECH").Should().Be("[TECH] [PEOPLE] hello");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Tag__Should_ignore_null_or_empty(string value)
        {
            "hello".Tag(value).Should().Be("hello");
        }

        [Fact]
        public void TagSemantic__Should_tag_one_element()
        {
            "hello".TagSemantic("field", "TECH").Should().Be("[{field}] hello");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void TagSemantic__Should_ignore_null_or_empty(string value)
        {
            "hello".TagSemantic("field", value).Should().Be("{field}hello");
        }

        [Fact]
        public void Indent__Should_indent_using_tab_When_useSpaces_is_false()
        {
            // Arrange
            var text = $"This is first line{Environment.NewLine}Second line";

            text.Indent(false).Should().Be($"\tThis is first line{Environment.NewLine}\tSecond line");
        }

        [Fact]
        public void Indent__Should_indent_using_spaces_When_useSpaces_is_true()
        {
            // Arrange
            var text = $"This is first line{Environment.NewLine}Second line";

            text.Indent().Should().Be($"    This is first line{Environment.NewLine}    Second line");
        }
    }
}