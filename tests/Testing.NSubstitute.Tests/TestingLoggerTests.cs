using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Exceptions;
using RecShark.Extensions;
using RecShark.Testing.FluentAssertions.Tests;
using Xunit;

namespace RecShark.Testing.NSubstitute.Tests
{
    public class LogExtensionsTests
    {
        private readonly TestingLogger logger = Substitute.For<TestingLogger>();

        [Fact]
        public void Logged__Should_receive_log()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.Logged(LogLevel.Error, "error!");
            logger.Logged(Arg.Any<LogLevel>(), "error!");
        }

        [Fact]
        public void Logged__Should_fails_When_not_found()
        {
            // Act
            Action action = () => logger.Logged(LogLevel.Error, "error!");

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage(@"*Log(<null>, Error, ~""error!"", <null>)*");
        }

        [Trait("category", "count")]
        [Fact]
        public void Logged__Should_manage_count()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");
            logger.Log(LogLevel.Error, "error!");
            logger.Log(LogLevel.Error, "something else");
            logger.Log(LogLevel.Information, "error!");

            // Act
            logger.Logged(LogLevel.Error, "error!", count: 2);
        }

        [Trait("category", "count")]
        [Fact]
        public void Logged__Should_manage_count_With_failure()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            Action action = () => logger.Logged(LogLevel.Error, "error!", count: 2);

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage("*Expected to receive exactly 2 calls matching*");
        }

        [Trait("category", "multiline")]
        [Fact]
        public void Logged__Should_manage_multiline()
        {
            // Arrange
            logger.Log(
                LogLevel.Error,
                @"error!
... details ...");

            // Act
            logger.Logged(LogLevel.Error, $"error!{Environment.NewLine}... details ...");
            logger.Logged(Arg.Any<LogLevel>(), $"error!{Environment.NewLine}... details ...");
            logger.Logged(LogLevel.Error);
        }

        [Trait("category", "multiline")]
        [Fact]
        public void Logged__Should_manage_multiline_With_failure()
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
                  .WithMessage(@"*Log(<null>, Error, ~""error!"", <null>)*");
        }

        [Trait("category", "wildcard")]
        [Fact]
        public void Logged__Should_manage_wildcard()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.Logged(LogLevel.Error, "*ror*");
            logger.Logged(Arg.Any<LogLevel>(), "*ror*");
        }

        [Trait("category", "wildcard")]
        [Trait("category", "multiline")]
        [Fact]
        public void Logged__Should_manage_wildcard_and_multiline()
        {
            // Arrange
            logger.Log(
                LogLevel.Error,
                @"error!
... details ...");

            // Act
            logger.Logged(LogLevel.Error, "*ror*");
            logger.Logged(Arg.Any<LogLevel>(), "*ror*");
        }

        [Trait("category", "wildcard")]
        [Trait("category", "count")]
        [Fact]
        public void Logged__Should_manage_wildcard_and_count()
        {
            // Arrange
            logger.Log(LogLevel.Error, "Error!");
            logger.Log(LogLevel.Error, "error!");
            logger.Log(LogLevel.Error, "something else");
            logger.Log(LogLevel.Information, "error!");

            // Act
            logger.Logged(LogLevel.Error, "*ror*", count: 2);
        }

        [Trait("category", "wildcard")]
        [Fact]
        public void Logged__Should_manage_wildcard_With_failure()
        {
            // Act
            Action action = () => logger.Logged(LogLevel.Error, "*ror*");

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage(@"*Log(<null>, Error, ~""*ror*"", <null>)*");
        }

        [Trait("category", "wildcard")]
        [Fact]
        public void Logged__Should_manage_wildcard_and_count_With_failure()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            Action action = () => logger.Logged(LogLevel.Error, "*ror*", count: 2);

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage("*Expected to receive exactly 2 calls matching*");
        }

        [Trait("category", "arg params")]
        [Fact]
        public void Logged__Should_manage_arg_params()
        {
            // Arrange
            var exception = new Exception("an error occured!");
            logger.Log(LogLevel.Error, exception, "error!");

            // Act
            logger.Logged(Arg.Any<Exception>(), LogLevel.Error, "error!", count: 1);
            logger.Logged(Arg.Any<Exception>(), Arg.Any<LogLevel>());
            logger.Logged(Arg.Is<Exception>(e => e.Message.Contains("error")), LogLevel.Error, "error!", count: 1);
            logger.Logged(Arg.Is<Exception>(e => e.Message.Contains("error")), Arg.Any<LogLevel>());
        }

        [Trait("category", "scope")]
        [Fact]
        public void Logged__Should_manage_scope()
        {
            // Arrange
            using (logger.WithScope(("id", "5")))
            {
                logger.Log(LogLevel.Error, "{name} error!", "jason");
            }

            // Act
            logger.Logged(LogLevel.Error, "jason error!");
            logger.Logged(LogLevel.Error, "jason error!", "id=5");
        }

        [Trait("category", "scope")]
        [Fact]
        public void Logged__Should_manage_scopes()
        {
            // Arrange
            using (logger.WithScope(("id", "5"), ("ex", "test")))
            using (logger.WithScope(("other", "x")))
            {
                logger.Log(LogLevel.Error, "{name} error!", "jason");
            }

            // Act
            logger.Logged(LogLevel.Error, "jason error!");
            logger.Logged(LogLevel.Error, "jason error!", "id=5__*");
            logger.Logged(LogLevel.Error, "jason error!", "id=5__ex=test__other=x");
        }

        [Trait("category", "scope")]
        [Theory]
        [InlineData("id=1")]
        [InlineData("id=5")]
        [InlineData("id=5__ex=test__other=y")]
        [InlineData("*__ex=test2__*")]
        public void Logged__Should_manage_scope_With_failure(string scope)
        {
            // Arrange
            using (logger.WithScope(("id", "5"), ("ex", "test")))
            using (logger.WithScope(("other", "x")))
            {
                logger.Log(LogLevel.Error, "{name} error!", "jason");
            }

            // Act
            Action action = () => logger.Logged(LogLevel.Error, "jason error!", scope);

            // Assert
            action.Should().Throw<ReceivedCallsException>()
                  .WithMessage($@"*Log(<null>, Error, ~""jason error!"", ~""{scope}"")*");
        }

        [Trait("category", "scope")]
        [Fact]
        public void Logged__Should_manage_scope_With_dynamic_info()
        {
            // Arrange
            var item = new ObjectForTests(1, 3, DateTime.Now);
            using (logger.WithScope(("id", item.ToAccessor(x => x.Id))))
            {
                logger.Log(LogLevel.Error, "{name} error!", "jason");
                item.Id = 5;
                logger.Log(LogLevel.Error, "{name} error!", "jason");
            }

            // Act
            logger.Logged(LogLevel.Error, "jason error!", count: 2);
            logger.Logged(LogLevel.Error, "jason error!", "id=1");
            logger.Logged(LogLevel.Error, "jason error!", "id=5");
        }

        [Trait("category", "scope")]
        [Fact]
        public async Task Logged__Should_manage_scope_With_multiple_thread()
        {
            // Arrange
            Task LogNums(int ratio, string name)
            {
                using (logger.WithScope(("name", name)))
                {
                    for (var i = 1; i <= 3; i++)
                    {
                        logger.LogInformation("{number}", ratio * i);
                        Thread.Sleep(500);
                    }
                }
                return Task.CompletedTask;
            }

            using (logger.WithScope(("scope", "//")))
            {
                logger.LogInformation("Starting ...");

                await Task.WhenAll(
                    LogNums(1, "pos"),
                    LogNums(-1, "neg")
                );

                logger.LogInformation("Finished");
            }

            // Act
            logger.Logged(LogLevel.Information, "Starting ...", "scope=//");
            logger.Logged(LogLevel.Information, "Finished", "scope=//");
            logger.Logged(LogLevel.Information, "1", "scope=//__name=pos");
            logger.Logged(LogLevel.Information, "2", "scope=//__name=pos");
            logger.Logged(LogLevel.Information, "3", "scope=//__name=pos");
            logger.Logged(LogLevel.Information, "-1", "scope=//__name=neg");
            logger.Logged(LogLevel.Information, "-2", "scope=//__name=neg");
            logger.Logged(LogLevel.Information, "-3", "scope=//__name=neg");
        }

        [Trait("category", "scope")]
        [Fact]
        public void Logged__Should_manage_scope_With_parallelization()
        {
            // Arrange
            void LogNums(int n)
            {
                using (logger.WithScope(("name", n)))
                {
                    for (var i = 1; i <= 2; i++)
                    {
                        logger.LogInformation("{number}", 10*n+i);
                        Thread.Sleep(100);
                    }
                }
            }

            using (logger.WithScope(("scope", "//")))
            {
                logger.LogInformation("Starting ...");

                var numbers = Enumerable.Range(1, 5).ToList();
                Parallel.ForEach(numbers, new ParallelOptions() {MaxDegreeOfParallelism = 3}, LogNums);

                logger.LogInformation("Finished");
            }

            // Act
            logger.Logged(LogLevel.Information, "Starting ...", "scope=//");
            logger.Logged(LogLevel.Information, "Finished", "scope=//");
            logger.Logged(LogLevel.Information, "11", "scope=//__name=1");
            logger.Logged(LogLevel.Information, "12", "scope=//__name=1");
            logger.Logged(LogLevel.Information, "21", "scope=//__name=2");
            logger.Logged(LogLevel.Information, "22", "scope=//__name=2");
            logger.Logged(LogLevel.Information, "31", "scope=//__name=3");
            logger.Logged(LogLevel.Information, "32", "scope=//__name=3");
            logger.Logged(LogLevel.Information, "41", "scope=//__name=4");
            logger.Logged(LogLevel.Information, "42", "scope=//__name=4");
            logger.Logged(LogLevel.Information, "51", "scope=//__name=5");
            logger.Logged(LogLevel.Information, "52", "scope=//__name=5");
        }

        [Fact]
        public void DidNotLog__Should_not_receive_log()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.DidNotLog(LogLevel.Error, "other");
            logger.DidNotLog(LogLevel.Information, "error!");
            logger.DidNotLog(Arg.Any<LogLevel>(), "other");
            logger.DidNotLog(LogLevel.Information);
        }

        [Fact]
        public void DidNotLog__Should_fails_When_found()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            Action action = () => logger.DidNotLog(LogLevel.Error, "error!");

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage(@"Expected to receive no calls matching*Log(<null>, Error, ~""error!"", <null>)*");
        }

        [Trait("category", "wildcard")]
        [Fact]
        public void DidNotLog__Should_manage_wildcard()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            logger.DidNotLog(LogLevel.Error, "o*er");
        }

        [Trait("category", "wildcard")]
        [Fact]
        public void DidNotLog__Should_manage_wildcard_With_failure()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");

            // Act
            Action action = () => logger.DidNotLog(LogLevel.Error, "e*or!");

            // Assert
            action.Should()
                  .Throw<ReceivedCallsException>()
                  .WithMessage(@"Expected to receive no calls matching*Log(<null>, Error, ~""e*or!"", <null>)*");
        }

        [Trait("category", "inOrder")]
        [Fact]
        public void LoggedInOrder__Should_receive_logs_in_order()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");
            logger.Log(LogLevel.Warning, "warning!");
            logger.Log(LogLevel.Information, "information!");

            // Act
            Received.InOrder(() =>
            {
                logger.Logged(LogLevel.Error, "error!");
                logger.Logged(LogLevel.Warning, "warning!");
                logger.Logged(LogLevel.Information, "information!");
            });
        }

        [Trait("category", "inOrder")]
        [Fact]
        public void LoggedInOrder__Should_fails_When_not_in_order()
        {
            // Arrange
            logger.Log(LogLevel.Error, "error!");
            logger.Log(LogLevel.Warning, "warning!");
            logger.Log(LogLevel.Information, "information!");

            // Act
            Action action = () => Received.InOrder(() =>
            {
                logger.Logged(LogLevel.Information, "information!");
                logger.Logged(LogLevel.Warning, "warning!");
                logger.Logged(LogLevel.Error, "error!");
            });

            // Assert
            action.Should()
                  .Throw<CallSequenceNotFoundException>()
                  .WithMessage(@"*Actually received matching calls in this order:

    Log(<null>, Error, ""error!"", """")
    Log(<null>, Warning, ""warning!"", """")
    Log(<null>, Information, ""information!"", """")*");
        }
    }
}
