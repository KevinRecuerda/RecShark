using System.Diagnostics;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Data.Db.Document.Initialization;
using RecShark.Extensions.DependencyInjection;
using RecShark.Extensions.DependencyInjection.Testing;

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
