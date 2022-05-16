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
                  .WithMessage(@"*Log(<null>, Error, <null>, ~""error!"")*");
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
                  .WithMessage(@"*Log(<null>, Error, <null>, ~""error!"")*");
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
                  .WithMessage(@"*Log(<null>, Error, <null>, ~""*ror*"")*");
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

        [Trait("category", "special char")]
        [Theory]
        [InlineData("[TAG] msg", "[T*G] msg")]
        [InlineData("{TAG} msg", "{T*G} msg")]
        [InlineData("(TAG) msg", "(T*G) msg")]
        [InlineData("|TAG| msg", "|T*G| msg")]
        [InlineData("+TAG+ msg", "+T*G+ msg")]
        [InlineData("#TAG# msg", "#T*G# msg")]
        public void Logged__Should_manage_special_char(string log, string pattern)
        {
            // Arrange
            logger.Log(LogLevel.Error, log);

            // Act
            logger.Logged(LogLevel.Error, pattern);
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

        [Trait("category", "template")]
        [Fact]
        public void Logged__Should_manage_template()
        {
            // Arrange
            logger.Log(LogLevel.Error, "{name} error!", "jason");

            // Act
            logger.Logged(LogLevel.Error, "jason error!");
            logger.LoggedScope(LogLevel.Error, "name=jason", "jason error!");
        }

        [Trait("category", "template")]
        [Trait("category", "scope")]
        [Fact]
        public void Logged__Should_manage_template_and_scope()
        {
            // Arrange
            using (logger.WithScope(("name", "context")))
                logger.Log(LogLevel.Error, "{name} error!", "jason");

            // Act
            logger.Logged(LogLevel.Error, "jason error!");
            logger.LoggedScope(LogLevel.Error, "name=jason", "jason error!");
        }

        [Trait("category", "scope")]
        [Fact]
        public void Logged__Should_manage_scope()
        {
            // Arrange
            using (logger.WithScope(("id", "5")))
            {
                logger.Log(LogLevel.Error, "jason error!");
            }

            // Act
            logger.Logged(LogLevel.Error, "jason error!");
            logger.LoggedScope(LogLevel.Error, "id=5", "jason error!");
        }

        [Trait("category", "scope")]
        [Fact]
        public void Logged__Should_manage_scopes()
        {
            // Arrange
            using (logger.WithScope(("id", "5"), ("ex", "test")))
            using (logger.WithScope(("other", "x")))
            {
                logger.Log(LogLevel.Error, "jason error!");
            }

            // Act
            logger.Logged(LogLevel.Error, "jason error!");
            logger.LoggedScope(LogLevel.Error, "id=5__*", "jason error!");
            logger.LoggedScope(LogLevel.Error, "id=5__ex=test__other=x", "jason error!");
        }

        [Trait("category", "scope")]
        [Fact]
        public void Logged__Should_manage_scopes_override()
        {
            // Arrange
            using (logger.WithScope(("id", "5")))
            using (logger.WithScope(("id", "3")))
            {
                logger.Log(LogLevel.Error, "{id} error!", 1);
            }

            // Act
            logger.LoggedScope(LogLevel.Error, "id=1", "1 error!");
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
                logger.Log(LogLevel.Error, "jason error!");
            }

            // Act
            Action action = () => logger.LoggedScope(LogLevel.Error, scope, "jason error!");

            // Assert
            action.Should().Throw<ReceivedCallsException>()
                  .WithMessage($@"*Log(<null>, Error, ~""{scope}"", ~""jason error!"")*");
        }

        [Trait("category", "scope")]
        [Fact]
        public void Logged__Should_manage_scope_With_dynamic_info()
        {
            // Arrange
            var item = new ObjectForTests(1, 3.14, DateTime.Now);
            using (logger.WithScope(("id", item.ToAccessor(x => x.Id))))
            {
                logger.Log(LogLevel.Error, "jason error!");
                item.Id = 5;
                logger.Log(LogLevel.Error, "jason error!");
            }

            // Act
            logger.Logged(LogLevel.Error, "jason error!", count: 2);
            logger.LoggedScope(LogLevel.Error, "id=1", "jason error!");
            logger.LoggedScope(LogLevel.Error, "id=5", "jason error!");
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
                        logger.LogInformation($"{ratio * i}");
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
            logger.LoggedScope(LogLevel.Information, "scope=//", "Starting ...");
            logger.LoggedScope(LogLevel.Information, "scope=//", "Finished");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=pos", "1");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=pos", "2");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=pos", "3");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=neg", "-1");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=neg", "-2");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=neg", "-3");
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
                        logger.LogInformation($"{10 * n + i}");
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
            logger.LoggedScope(LogLevel.Information, "scope=//", "Starting ...");
            logger.LoggedScope(LogLevel.Information, "scope=//", "Finished");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=1", "11");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=1", "12");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=2", "21");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=2", "22");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=3", "31");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=3", "32");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=4", "41");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=4", "42");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=5", "51");
            logger.LoggedScope(LogLevel.Information, "scope=//__name=5", "52");
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
                  .WithMessage(@"Expected to receive no calls matching*Log(<null>, Error, <null>, ~""error!"")*");
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
                  .WithMessage(@"Expected to receive no calls matching*Log(<null>, Error, <null>, ~""e*or!"")*");
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

    Log(<null>, Error, """", ""error!"")
    Log(<null>, Warning, """", ""warning!"")
    Log(<null>, Information, """", ""information!"")*");
        }
    }
}
