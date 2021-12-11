using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Exceptions;
using Xunit;

namespace RecShark.Testing.NSubstitute.Tests
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

        [Trait("category", "multiline")]
        [Fact]
        public void Logged__Should_handle_multiline()
        {
            // Arrange
            logger.Log(
                LogLevel.Error,
                @"error!
... details ...");

            // Act
            logger.Logged(LogLevel.Error, $"error!{Environment.NewLine}... details ...");
            logger.Logged(null,           $"error!{Environment.NewLine}... details ...");
            logger.Logged(LogLevel.Error, null);
        }

        [Trait("category", "multiline")]
        [Fact]
        public void Logged__Should_throw_exception_When_multiline_does_not_match()
        {
            // Arrange
            logger.Log(
                LogLevel.Error,
                @"error!
... details ...");

            // Act
            Action action = () => logger.Logged(LogLevel.Error, "error!");

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage("*Log<Object>(Error, any EventId, error!, <null>, any Func<Object, Exception, String>)*");
        }

        [Trait("category", "wildcard")]
        [Fact]
        public void Logged__Should_receive_log_when_use_wildcard()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.Logged(LogLevel.Error, "*ror*");
            logger.Logged(null,           "*ror*");
        }

        [Trait("category", "wildcard")]
        [Trait("category", "multiline")]
        [Fact]
        public void Logged__Should_handle_multiline_when_use_wildcard()
        {
            // Arrange
            logger.Log(
                LogLevel.Error,
                @"error!
... details ...");

            // Act
            logger.Logged(LogLevel.Error, "*ror*");
            logger.Logged(null,           "*ror*");
        }

        [Trait("category", "wildcard")]
        [Fact]
        public void Logged__Should_receive_correct_count_when_use_wildcard()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.Logged(LogLevel.Error, "*ror*", 2);
        }

        [Trait("category", "wildcard")]
        [Fact]
        public void Logged__Should_throw_exception_displaying_message_when_use_wildcard()
        {
            // Arrange

            // Act
            Action action = () => logger.Logged(LogLevel.Error, "*ror*", 2);

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage("*Log<Object>(Error, any EventId, *ror*, <null>, any Func<Object, Exception, String>)*");
        }

        [Trait("category", "wildcard")]
        [Fact]
        public void Logged__Should_throw_exception_when_count_is_incorrect_when_use_wildcard()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            Action action = () => logger.Logged(LogLevel.Error, "*ror*", 2);

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage("*Expected to receive exactly 2 calls matching*");
        }

        [Trait("category", "arg params")]
        [Fact]
        public void Logged__Should_receive_log_when_use_arg_params()
        {
            // Arrange
            var exception = new Exception("an error occured!");
            logger.Log(LogLevel.Error, exception, "error!");

            // Act
            logger.Logged(Arg.Any<Exception>(), LogLevel.Error, "error!", 1);
            logger.Logged(Arg.Any<Exception>());
            logger.Logged(Arg.Is<Exception>(e => e.Message.Contains("error")), LogLevel.Error, "error!", 1);
            logger.Logged(Arg.Is<Exception>(e => e.Message.Contains("error")));
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

        [Trait("category", "wildcard")]
        [Fact]
        public void DidNotLog__Should_not_receive_log_when_use_wildcard()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.DidNotLog(LogLevel.Error, "o*er");
        }

        [Trait("category", "wildcard")]
        [Fact]
        public void DidNotLog__Should_throw_exception_displaying_message_when_use_wildcard()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            Action action = () => logger.DidNotLog(LogLevel.Error, "e*or!");

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage(
                       "Expected to receive no calls matching*Log<Object>(Error, any EventId, e*or!, <null>, any Func<Object, Exception, String>)*");
        }

        [Trait("category", "inOrder")]
        [Fact]
        public void LoggedInOrder__Should_handle_inOrder()
        {
            // Arrange
            logger.Log(LogLevel.Error,       "error!");
            logger.Log(LogLevel.Warning,     "warning!");
            logger.Log(LogLevel.Information, "information!");

            // Act
            logger.LoggedInOrder(
                x =>
                {
                    x.Logged(LogLevel.Error,       "error!");
                    x.Logged(LogLevel.Warning,     "warning!");
                    x.Logged(LogLevel.Information, "information!");
                });
        }
    }
}
