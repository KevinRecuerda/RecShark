﻿using FluentAssertions;
 using Xunit;

 namespace RecShark.Extensions.Tests
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void Clone__Should_clone_object()
        {
            // Arrange
            var expected = new ObjectToClone { Index = 1, Message = "Hello World !" };
            
            // Act
            var actual = expected.Clone();
            expected.Message = "Message changed !";

            // Assert
            actual.Should().NotBeEquivalentTo(expected);
            actual.Message.Should().Be("Hello World !");
        }
        
        private class ObjectToClone
        {
            public int    Index   { get; set; }
            public string Message { get; set; }
        }
    }
}
