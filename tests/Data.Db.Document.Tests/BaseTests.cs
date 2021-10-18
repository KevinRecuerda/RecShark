using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Data.Db.Document.Initialization;
using RecShark.Data.Db.Document.Testing;
using RecShark.Data.Db.Document.Tests.Initialization;
using RecShark.Data.Db.Document.Tests.MartenExtensions;
using RecShark.Extensions.DependencyInjection;
using RecShark.Extensions.DependencyInjection.Testing;
using Xunit;

namespace RecShark.Data.Db.Document.Tests
{
    [CollectionDefinition(nameof(DataLockCollection), DisableParallelization = true)]

    [Collection(nameof(DataLockCollection))]
    public class BaseDocTests : IntegrationTests<DocHooks>
    {
        protected BaseDocTests(DocHooks hooks = null) : base(hooks) { }
    }

    public class DocHooks : DocumentDataHooks
    {
        public DocHooks() : base(new DocModule()) { }
    }

    public class DocModule : DIModule
    {
        public override DIModule[] Dependencies => new DIModule[] {new DocumentStoreDataModule<DocumentStoreFactory, SampleConnectionString>()};

        public override void Load(IServiceCollection services) { }
    }

    public class SampleConnectionString : ConnectionString
    {
        public SampleConnectionString(IConfiguration configuration) : base("sample", configuration) { }
    }

    public class DocumentStoreFactory : BaseDocumentStoreFactory
    {
        public DocumentStoreFactory(IConnectionString connectionString) : base(connectionString) { }

        public override string Schema { get; set; } = "document_store";

        public override DataChange[] DataChanges => new DataChange[] {new ObjectDataChange()};

        public bool ConfigureCalled { get; private set; }

        protected override void Configure(StoreOptions options)
        {
            ConfigureCalled = true;

            options.Schema.For<ObjectForTests>().DatabaseSchemaName(Schema);

            options.Schema.For<Item>().DatabaseSchemaName(Schema);
            options.Schema.For<Control>()
                   .ForeignKey<Item>(c => c.ItemId)
                   .Index(c => c.Date)
                   .DatabaseSchemaName(Schema);

            options.Storage.Add<Views>();
        }
    }
}
