using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void Clone__Should_clone_object()
        {
            // Arrange
            var item = new ObjectToClone { Index = 1, Message = "Hello World !" };

            // Act
            var actual = item.Clone();
            item.Message = "Message changed !";

            // Assert
            actual.Should().NotBeEquivalentTo(item);
            actual.Message.Should().Be("Hello World !");
        }

        [Fact]
        public void Clone__Should_clone_dico()
        {
            // Arrange
            var item = new Dictionary<string, ObjectToClone>
            {
                ["A"] = new ObjectToClone { Index = 1, Message = "Hello World !" },
                ["B"] = new ObjectToClone { Index = 2, Message = "Hello Team !" }
            };

            // Act
            var actual = item.Clone();
            item["A"].Message = "Message changed !";

            // Assert
            actual.Should().NotBeSameAs(item);
            actual["A"].Message.Should().Be("Hello World !");
        }

        [Fact]
        public void Clone__Should_clone_primitive_type()
        {
            1.Clone().Should().Be(1);
        }

        [Fact]
        public void Clone__Should_clone_datetime()
        {
            new DateTime(2000, 12, 31).Clone().ToIso().Should().Be("2000-12-31");
        }

        private class ObjectToClone
        {
            public int Index { get; set; }
            public string Message { get; set; }
        }
    }
}
