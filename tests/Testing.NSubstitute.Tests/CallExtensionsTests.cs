using System;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Exceptions;
using Xunit;

namespace RecShark.Testing.NSubstitute.Tests
{
    public class CallExtensionsTests
    {
        private readonly IServiceForTests service = Substitute.For<IServiceForTests>();

        [Fact]
        public void DidNotReceiveAnyCall__Should_receive_nothing()
        {
            service.DidNotReceiveAnyCall();
        }

        [Fact]
        public void DidNotReceiveAnyCall__Should_throw_exception_when_received_at_least_one_call()
        {
            // Arrange
            service.Do(null);
            service.Do(null);
            
            // Act
            Action action = () => service.DidNotReceiveAnyCall();

            // Assert
            action.Should().Throw<ReceivedCallsException>()
                .WithMessage("Expected to receive no calls, but received 2 calls");
        }
    }
}