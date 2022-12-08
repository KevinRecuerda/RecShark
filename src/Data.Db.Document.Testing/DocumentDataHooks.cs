using System;
using System.Diagnostics;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Data.Db.Document.Initialization;
using RecShark.DependencyInjection;
using RecShark.Testing.DependencyInjection;
using Weasel.Core;

namespace RecShark.Data.Db.Document.Testing
{
    public class DocumentDataHooks : Hooks
    {
        protected DocumentDataHooks(params DIModule[] modules) : base(modules)
        {
            var factory        = (BaseDocumentStoreFactory) Provider.GetService<IDocumentStoreFactory>();
            var testingFactory = new DocumentStoreFactoryTesting(factory);

            Services.Reset(ServiceDescriptor.Singleton<IDocumentStoreFactory>(testingFactory));
        }

        public IDocumentCleaner Cleaner => Provider.GetService<IDocumentStore>().Advanced.Clean;
    }

    public class DocumentStoreFactoryTesting : IDocumentStoreFactory
    {
        public DocumentStoreFactoryTesting(BaseDocumentStoreFactory innerFactory)
        { 
            /*
             * TODO: this a workaround to avoid below error
             * Cannot write DateTimeOffset with Offset=01:00:00 to PostgreSQL type 'timestamp with time zone', only offset 0 (UTC) is supported.
             * Other solution: set app TZ to UTC ?
             * //TODO: check where should call this
             */
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            
            InnerFactory = innerFactory;

            InnerFactory.Schema += "_tests";
        }

        public BaseDocumentStoreFactory InnerFactory { get; }

        public string       Schema      => InnerFactory.Schema;
        public DataChange[] DataChanges => InnerFactory.DataChanges;

        public IDocumentStore CreateDocumentStore()
        {
            var store = DocumentStore.For(ConfigureDocumentStore);
            return store;
        }

        private void ConfigureDocumentStore(StoreOptions options)
        {
            InnerFactory.ConfigureDocumentStore(options);

            options.AutoCreateSchemaObjects = AutoCreate.All;

            if (Debugger.IsAttached)
                options.Logger(new ConsoleMartenLogger());
        }
    }
}
