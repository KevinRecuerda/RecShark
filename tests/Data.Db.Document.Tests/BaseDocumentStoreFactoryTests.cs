using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Data.Db.Document.Initialization;
using RecShark.Data.Db.Document.Testing;
using Xunit;

namespace RecShark.Data.Db.Document.Tests
{
    public class BaseDocumentStoreFactoryTests : BaseDocTests
    {
        private readonly IDocumentStoreFactory factory;

        public BaseDocumentStoreFactoryTests()
        {
            this.factory = this.Hooks.Provider.GetService<IDocumentStoreFactory>();
        }

        [Fact]
        public void CreateDocumentStore__Should_use_correct_schema()
        {
            var actual = (DocumentStore) this.factory.CreateDocumentStore();

            actual.Options.DatabaseSchemaName.Should().Be(this.factory.Schema);
        }

        [Fact]
        public void CreateDocumentStore__Should_set_mapping_for_dataChange()
        {
            var actual = (DocumentStore) this.factory.CreateDocumentStore();

            actual.Storage.MappingFor(typeof(DataChange)).DatabaseSchemaName.Should().Be(this.factory.Schema);
            actual.Storage.MappingFor(typeof(DataLock)).DatabaseSchemaName.Should().Be(this.factory.Schema);
        }

        [Fact]
        public void CreateDocumentStore__Should_call_configuration()
        {
            this.factory.CreateDocumentStore();

            var innerFactory = (DocumentStoreFactory)((DocumentStoreFactoryTesting) this.factory).InnerFactory;
            innerFactory.ConfigureCalled.Should().BeTrue();
        }
    }
}