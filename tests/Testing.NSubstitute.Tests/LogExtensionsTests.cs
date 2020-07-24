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
            logger.Logged(null,           "error!");
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
            action.Should()
                  .Throw<ReceivedCallsException>()
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
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage("*Expected to receive exactly 2 calls matching*");
        }

        [Fact]
        public void LoggedMatching__Should_receive_log()
        {
            // Arrange
            var pattern = ".*ror.*";
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.LoggedMatching(LogLevel.Error, pattern);
            logger.LoggedMatching(null,           pattern);
        }

        [Fact]
        public void LoggedMatching__Should_receive_log_correct_count()
        {
            // Arrange
            var pattern = ".*ror.*";
            logger.Log(LogLevel.Error, "error!");
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.LoggedMatching(LogLevel.Error, pattern, 2);
        }

        [Fact]
        public void LoggedMatching__Should_throw_exception_displaying_message()
        {
            // Arrange
            var pattern = ".*ror.*";

            // Act
            Action action = () => logger.LoggedMatching(LogLevel.Error, pattern, 2);

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage("*Log<Object>(Error, any EventId, .*ror.*, <null>, any Func<Object, Exception, String>)*");
        }

        [Fact]
        public void LoggedMatching__Should_throw_exception_when_count_is_incorrect()
        {
            // Arrange
            var pattern = ".*ror.*";
            logger.Log(LogLevel.Error, "error!");

            // Act
            Action action = () => logger.LoggedMatching(LogLevel.Error, pattern, 2);

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage("*Expected to receive exactly 2 calls matching*");
        }

        [Fact]
        public void DidNotLog__Should_not_receive_log()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.DidNotLog(LogLevel.Error,       "other");
            logger.DidNotLog(LogLevel.Information, "error!");
            logger.DidNotLog(null,                 "other");
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
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage(
                       "Expected to receive no calls matching*Log<Object>(Error, any EventId, error!, <null>, any Func<Object, Exception, String>)*");
        }

        [Fact]
        public void DidNotLogMatching__Should_not_receive_log()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.DidNotLogMatching(LogLevel.Error,       "o.*er");
            logger.DidNotLogMatching(LogLevel.Information, "e[0-9]or!");
            logger.DidNotLogMatching(null,                 "other");
            logger.DidNotLogMatching(LogLevel.Information, null);
        }

        [Fact]
        public void DidNotLogMatching__Should_throw_exception_displaying_message()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            Action action = () => logger.DidNotLogMatching(LogLevel.Error, "e.*or!");

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage(
                       "Expected to receive no calls matching*Log<Object>(Error, any EventId, e.*or!, <null>, any Func<Object, Exception, String>)*");
        }
    }
}
