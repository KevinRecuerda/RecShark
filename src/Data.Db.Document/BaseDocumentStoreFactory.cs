using System;
using Marten;
using RecShark.Data.Db.Document.Initialization;
using RecShark.Data.Db.Document.MartenExtensions;
using Weasel.Core;

namespace RecShark.Data.Db.Document
{
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
            /*
            * TODO: this a workaround to avoid below error
            * Cannot write DateTimeOffset with Offset=01:00:00 to PostgreSQL type 'timestamp with time zone', only offset 0 (UTC) is supported.
            * Other solution: set app TZ to UTC ?
            * //TODO: check where should call this
            */
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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

            // TODO: check if need to enable PLV8 https://martendb.io/documents/plv8.html
            //options.Advanced.PLV8Enabled = false;

            options.Schema.For<DataChangeLog>().DatabaseSchemaName(Schema);
            options.Schema.For<DataLock>().DatabaseSchemaName(Schema);

            Configure(options);
        }

        protected abstract void Configure(StoreOptions options);
    }
}
