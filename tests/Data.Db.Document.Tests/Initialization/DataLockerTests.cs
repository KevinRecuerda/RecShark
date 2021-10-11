using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Data.Db.Document.Initialization;
using Xunit;

namespace RecShark.Data.Db.Document.Tests.Initialization
{
    [CollectionDefinition(nameof(DataLockCollection), DisableParallelization = true)]
    public class DataLockCollection { }

    [Trait("Category", "integration-tests")]
    [Collection(nameof(DataLockCollection))]
    public class DataLockerTests : BaseDocTests
    {
        private readonly DataLocker dataLocker;

        public DataLockerTests()
        {
            this.dataLocker = (DataLocker) this.Hooks.Provider.GetService<IDataLocker>();

            this.dataLocker.SleepTime = 0;
        }

        public override void Dispose()
        {
            this.Hooks.Cleaner.CompletelyRemove(typeof(DataLock));
        }

        [Fact]
        public async Task AcquireLock__Should_add_lock()
        {
            await this.dataLocker.AcquireLock();

            var actual = await this.dataLocker.GetLock();
            actual.Should().NotBeNull();
            actual.Id.Should().Be(1);
            actual.Host.Should().Be(Environment.MachineName);
        }

        [Fact]
        public async Task AcquireLock__Should_throw_exception_When_cannot_acquire_lock()
        {
            // Arrange
            await this.dataLocker.AcquireLock();

            // Act
            Task Action() => this.dataLocker.AcquireLock();

            // Assert
            var exception = await Assert.ThrowsAsync<DataLockException>(Action);
            exception.Message.Should().Be("Could not acquire database lock after 6 retry");
        }

        [Fact]
        public async Task TryLock__Should_lock()
        {
            (await this.dataLocker.TryLock()).Should().BeTrue();
            (await this.dataLocker.TryLock()).Should().BeFalse();
        }

        [Fact]
        public async Task ReleaseLock__Should_delete_lock()
        {
            // Arrange
            await this.dataLocker.AcquireLock();

            // Act
            await this.dataLocker.ReleaseLock();

            // Assert
            var actual = await this.dataLocker.GetLock();
            actual.Should().BeNull();
        }

        [Fact]
        public async Task DataLocker__Should_chain_correctly()
        {
            await this.dataLocker.AcquireLock();
            await this.dataLocker.ReleaseLock();

            await this.dataLocker.AcquireLock();
            await this.dataLocker.ReleaseLock();
        }
    }
}