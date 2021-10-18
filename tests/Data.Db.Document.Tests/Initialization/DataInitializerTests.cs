using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RecShark.Data.Db.Document.Initialization;
using RecShark.Testing.NSubstitute;
using Xunit;

namespace RecShark.Data.Db.Document.Tests.Initialization
{
    [Trait("Category", "integration-tests")]
    [Collection(nameof(DataLockCollection))]
    public class DataInitializerTests : BaseDocTests
    {
        public override void Dispose()
        {
            Hooks.Cleaner.CompletelyRemoveAll();
        }

        [Fact]
        public async Task Init__Should_lock_before_apply_changes()
        {
            // Arrange
            var factory         = Hooks.Provider.GetService<IDocumentStoreFactory>();
            var logger          = Substitute.For<ILogger<DataInitializer>>();
            var dataInitializer = Substitute.For<DataInitializer>(factory, logger);

            var locker = Substitute.For<IDataLocker>();
            dataInitializer.CreateDataLocker(Arg.Any<IDocumentStore>()).Returns(locker);

            // Act
            await dataInitializer.Init();

            // Assert
            logger.DidNotLog(LogLevel.Error);
            Received.InOrder(
                async () =>
                {
                    await locker.AcquireLock();
                    await dataInitializer.ApplyChanges(Arg.Any<IDocumentStore>(), Arg.Any<DataChange[]>(), ExecutionMode.PreSchemaChanges);
                    await dataInitializer.ApplyChanges(Arg.Any<IDocumentStore>(), Arg.Any<DataChange[]>(), ExecutionMode.PostSchemaChanges);
                    await locker.ReleaseLock();
                });
        }

        [Fact]
        public async Task Init__Should_apply_changes_pre_changes_before_modifying_schema()
        {
            // Arrange
            var factory = Substitute.For<IDocumentStoreFactory>();
            var store   = Substitute.For<IDocumentStore>();
            var schema  = Substitute.For<IDocumentSchema>();

            store.Schema.Returns(schema);
            factory.CreateDocumentStore().Returns(store);

            var dataChanges = new DataChange[] {new ObjectDataChange()};
            factory.DataChanges.Returns(dataChanges);

            var logger          = Substitute.For<ILogger<DataInitializer>>();
            var dataInitializer = Substitute.For<DataInitializer>(factory, logger);

            // Act
            await dataInitializer.Init();

            // Assert
            Received.InOrder(
                async () =>
                {
                    await dataInitializer.ApplyChanges(store, dataChanges.AsArg(), ExecutionMode.PreSchemaChanges);
                    schema.ApplyAllConfiguredChangesToDatabase();
                    await dataInitializer.ApplyChanges(store, dataChanges.AsArg(), ExecutionMode.PostSchemaChanges);
                });
        }

        [Fact]
        public async Task Init__Should_initialize_changes()
        {
            // Arrange
            var store           = Hooks.Provider.GetService<IDocumentStore>();
            var dataInitializer = Hooks.Provider.GetService<IDataInitializer>();

            // Act
            await dataInitializer.Init();

            // Assert
            var actual = await DataInitializer.GetDataChanges(store);
            actual.Should().HaveCount(1);
            actual[0].Id.Should().Be("change");

            (await store.QuerySession().Query<ObjectForTests>().CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task ApplyChanges__Should_save_changes()
        {
            // Arrange
            var store           = Hooks.Provider.GetService<IDocumentStore>();
            var dataInitializer = Hooks.Provider.GetService<IDataInitializer>() as DataInitializer;

            var change1 = new ObjectDataChange();
            var change2 = new AnotherObjectDataChange();

            // Act
            await dataInitializer.ApplyChanges(
                store,
                new DataChange[] {change1, change2},
                ExecutionMode.PostSchemaChanges);

            // Assert
            var actual = await DataInitializer.GetDataChanges(store);
            actual.Should().HaveCount(2);
            actual[0].Id.Should().Be(change1.Id);
            actual[1].Id.Should().Be(change2.Id);
        }

        [Fact]
        public async Task ApplyChanges__Should_do_nothing_if_no_changes()
        {
            // Arrange
            var dataInitializer = Hooks.Provider.GetService<IDataInitializer>() as DataInitializer;
            var store           = Substitute.For<IDocumentStore>();

            // Act
            await dataInitializer.ApplyChanges(store, new DataChange[0], ExecutionMode.PostSchemaChanges);

            // Assert
            store.DidNotReceive().OpenSession();
        }

        [Fact]
        public async Task GetChangesToExecute__Should_only_return_changes_with_matching_execution_mode_and_not_yet_executed()
        {
            // Arrange
            var factory         = Hooks.Provider.GetService<IDocumentStoreFactory>();
            var logger          = Substitute.For<ILogger<DataInitializer>>();
            var dataInitializer = Substitute.For<DataInitializer>(factory, logger);

            var change1 = new ObjectDataChange();
            var change2 = new AnotherObjectDataChange();
            var change3 = new ObjectDataChangeExecuteBeforeSchemaChanges();

            dataInitializer.MustExecuteChange(change1, null).Returns(true);
            dataInitializer.MustExecuteChange(change2, null).Returns(false);

            // Act
            var actual = await dataInitializer.GetChangesToExecute(
                             factory.CreateDocumentStore(),
                             new DataChange[] {change1, change2, change3},
                             ExecutionMode.PostSchemaChanges);

            // Assert
            actual.Should().HaveCount(1);
            actual.Single().Id.Should().Be(change1.Id);
        }

        [Fact]
        public void MustExecuteChange__Should_return_true_if_change_has_never_been_executed()
        {
            // Arrange
            var dataInitializer = Hooks.Provider.GetService<IDataInitializer>() as DataInitializer;

            var change = new DataChangeLog {Version = 2, PatchVersion = 3, RunAlways = false, ExecutionMode = ExecutionMode.PostSchemaChanges};

            // Act
            var actual = dataInitializer.MustExecuteChange(change, null);

            // Assert
            actual.Should().BeTrue();
        }

        [Theory]
        [InlineData(2, 2)]
        [InlineData(1, 5)]
        public void MustExecuteChange__Should_return_true_if_change_set_has_higher_version(
            int historicalVersion,
            int historicalPatchVersion)
        {
            // Arrange
            var dataInitializer = Hooks.Provider.GetService<IDataInitializer>() as DataInitializer;

            var change           = new DataChangeLog {Version = 2, PatchVersion                 = 3};
            var historicalChange = new DataChangeLog {Version = historicalVersion, PatchVersion = historicalPatchVersion};

            // Act
            var actual = dataInitializer.MustExecuteChange(change, historicalChange);

            // Assert
            actual.Should().BeTrue();
        }

        [Theory]
        [InlineData(2, 3)]
        [InlineData(2, 4)]
        [InlineData(3, 0)]
        public void MustExecuteChange__Should_return_false_if_change_has_lower_version(
            int historicalVersion,
            int historicalPatchVersion)
        {
            // Arrange
            var dataInitializer = Hooks.Provider.GetService<IDataInitializer>() as DataInitializer;

            var change           = new DataChangeLog {Version = 2, PatchVersion                 = 3};
            var historicalChange = new DataChangeLog {Version = historicalVersion, PatchVersion = historicalPatchVersion};

            // Act
            var actual = dataInitializer.MustExecuteChange(change, historicalChange);

            // Assert
            actual.Should().BeFalse();
        }

        [Theory]
        [InlineData(2, 3)]
        [InlineData(2, 4)]
        [InlineData(3, 0)]
        public void MustExecuteChange__Should_return_true_if_change_has_run_always_set_to_true(
            int historicalVersion,
            int historicalPatchVersion)
        {
            // Arrange
            var dataInitializer = Hooks.Provider.GetService<IDataInitializer>() as DataInitializer;

            var change = new DataChangeLog
                {Version = 2, PatchVersion = 3, RunAlways = true, ExecutionMode = ExecutionMode.PostSchemaChanges};
            var historicalChange = new DataChangeLog {Version = historicalVersion, PatchVersion = historicalPatchVersion};

            // Act
            var actual = dataInitializer.MustExecuteChange(change, historicalChange);

            // Assert
            actual.Should().BeTrue();
        }
    }
}
