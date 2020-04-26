using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Exceptions;
using Xunit;

namespace RecShark.Extensions.Testing.NSubstitute.Tests
{
    public class LogExtensionsTests
    {
        private readonly ILogger logger = Substitute.For<ILogger>();

        [Fact]
        public void Logged__Should_receive_log()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.Logged(LogLevel.Error, "error!");
            logger.Logged(null, "error!");
            logger.Logged(LogLevel.Error, null);
        }

        [Fact]
        public void Logged__Should_receive_correct_count()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");
            logger.Log(LogLevel.Error, "error!");
            
            // Act
            logger.Logged(LogLevel.Error, "error!", 2);
        }

        [Fact]
        public void Logged__Should_throw_exception_displaying_message()
        {
            // Act
            Action action = () => logger.Logged(LogLevel.Error, "error!", 2);
            
            // Assert
            action.Should().Throw<ReceivedCallsException>()
                .WithMessage("*Log<Object>(Error, any EventId, error!, <null>, any Func<Object, Exception, String>)*");
        }

        [Fact]
        public void Logged__Should_throw_exception_when_count_is_incorrect()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");
            
            // Act
            Action action = () => logger.Logged(LogLevel.Error, "error!", 2);
            
            // Assert
            action.Should().Throw<ReceivedCallsException>()
                .WithMessage("*Expected to receive exactly 2 calls matching*");
        }

        [Fact]
        public void DidNotLog__Should_not_receive_log()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.DidNotLog(LogLevel.Error, "other");
            logger.DidNotLog(LogLevel.Information, "error!");
            logger.DidNotLog(null, "other");
            logger.DidNotLog(LogLevel.Information, null);
        }

        [Fact]
        public void DidNotLog__Should_throw_exception_displaying_message()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");
            
            // Act
            Action action = () => logger.DidNotLog(LogLevel.Error, "error!");
            
            // Assert
            action.Should().Throw<ReceivedCallsException>()
                .WithMessage("Expected to receive no calls matching*Log<Object>(Error, any EventId, error!, <null>, any Func<Object, Exception, String>)*");
        }
    }
}