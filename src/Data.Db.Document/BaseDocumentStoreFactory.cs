using Marten;
using RecShark.Data.Db.Document.Initialization;
using RecShark.Data.Db.Document.MartenExtensions;

namespace RecShark.Data.Db.Document
{
    using Weasel.Core;

    public abstract class BaseDocumentStoreFactory : IDocumentStoreFactory
    {
        private readonly IConnectionString connectionString;

        protected BaseDocumentStoreFactory(IConnectionString connectionString)
        {
            this.connectionString = connectionString;
        }

        public abstract string Schema { get; set; }

        public virtual DataChange[] DataChanges => new DataChange[0];

        public IDocumentStore CreateDocumentStore()
        {
            var store = DocumentStore.For(ConfigureDocumentStore);
            return store;
        }

        public void ConfigureDocumentStore(StoreOptions options)
        {
            options.Connection(connectionString.Value);
            options.DatabaseSchemaName = Schema;

            options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
            options.Linq.MethodCallParsers.Add(new ContainsAny());
            options.Linq.MethodCallParsers.Add(new IsBetween());
            options.Linq.MethodCallParsers.Add(new IsIn());
            options.Linq.MethodCallParsers.Add(new SmartMatchAny());
            options.UseDefaultSerialization(EnumStorage.AsString);
            options.Advanced.HiloSequenceDefaults.MaxLo = 10;

            // TODO: check if need to enable PLV8
            //options.Advanced.PLV8Enabled = false;

            options.Schema.For<DataChangeLog>().DatabaseSchemaName(Schema);
            options.Schema.For<DataLock>().DatabaseSchemaName(Schema);

            Configure(options);
        }

        protected abstract void Configure(StoreOptions options);
    }
}
