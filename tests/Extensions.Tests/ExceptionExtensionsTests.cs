using System;
using FluentAssertions;
using Xunit;

namespace RecShark.Extensions.Tests
{
    public class ExceptionExtensionsTests
    {
        [Fact]
        public void Find__Should_return_first_exception_of_type_AggregateException()
        {
            // Arrange
            var exception = new Exception("Exception", new AggregateException("InnerException"));

            // Act
            var exceptionFound = exception.Find<AggregateException>();
            
            // Assert
            exceptionFound.Should().BeOfType<AggregateException>();
        }
        
        [Fact]
        public void Find__Should_return_null_when_no_AggregateException_found()
        {
            // Arrange
            var exception = new Exception("Exception");

            // Act
            var exceptionFound = exception.Find<AggregateException>();
            
            // Assert
            exceptionFound.Should().BeNull();
        }
        
        [Fact]
        public void Last__Should_return_last_exception()
        {
            new AggregateException(new ArithmeticException()).Last().Should().BeOfType<ArithmeticException>();
        }

        [Fact]
        public void SmartMessage__Should_return_exception_message()
        {
            // Arrange
            var exception = new Exception("Exception");

            // Act
            var message = exception.SmartMessage();
            
            // Assert
            message.Should().Be("Exception");
        }
        
        [Fact]
        public void SmartMessage__Should_all_return_exception_message()
        {
            var message = new AggregateException(new ArithmeticException(), new DivideByZeroException())
                .SmartMessage();

            message.Should().Be(
                                @"Overflow or underflow in the arithmetic operation.
Attempted to divide by zero.");
        }
    }
}
