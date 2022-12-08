using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Data.Db.Document.Initialization;
using RecShark.Data.Db.Document.Testing;
using Xunit;

namespace RecShark.Data.Db.Document.Tests
{
    using Marten.Internal.Sessions;

    public class BaseDocumentStoreFactoryTests : BaseDocTests
    {
        private readonly IDocumentStoreFactory factory;

        public BaseDocumentStoreFactoryTests()
        {
            factory = Hooks.Provider.GetService<IDocumentStoreFactory>();
        }

        [Fact]
        public void CreateDocumentStore__Should_use_correct_schema()
        {
            var actual = (DocumentStore) factory.CreateDocumentStore();

            actual.Options.DatabaseSchemaName.Should().Be(factory.Schema);
        }

        [Fact]
        public void CreateDocumentStore__Should_set_mapping_for_dataChange()
        {
            var actual = (DocumentStore) factory.CreateDocumentStore();

            ((QuerySession) actual.QuerySession()).StorageFor(typeof(DataChange)).TableName.Schema.Should().Be(factory.Schema);
            ((QuerySession) actual.QuerySession()).StorageFor(typeof(DataLock)).TableName.Schema.Should().Be(factory.Schema);
        }

        [Fact]
        public void CreateDocumentStore__Should_call_configuration()
        {
            factory.CreateDocumentStore();

            var innerFactory = (DocumentStoreFactory)((DocumentStoreFactoryTesting) factory).InnerFactory;
            innerFactory.ConfigureCalled.Should().BeTrue();
        }
    }
}
