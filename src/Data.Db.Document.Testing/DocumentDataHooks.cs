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
            var factory        = (BaseDocumentStoreFactory) this.Provider.GetService<IDocumentStoreFactory>();
            var testingFactory = new DocumentStoreFactoryTesting(factory);

            this.Services.Reset(ServiceDescriptor.Singleton<IDocumentStoreFactory>(testingFactory));
        }

        public IDocumentCleaner Cleaner => this.Provider.GetService<IDocumentStore>().Advanced.Clean;
    }

    public class DocumentStoreFactoryTesting : IDocumentStoreFactory
    {
        public DocumentStoreFactoryTesting(BaseDocumentStoreFactory innerFactory)
        {
            this.InnerFactory = innerFactory;

            this.InnerFactory.Schema += "_tests";
        }

        public BaseDocumentStoreFactory InnerFactory { get; }

        public string       Schema      => this.InnerFactory.Schema;
        public DataChange[] DataChanges => this.InnerFactory.DataChanges;

        public IDocumentStore CreateDocumentStore()
        {
            var store = DocumentStore.For(this.ConfigureDocumentStore);
            return store;
        }

        private void ConfigureDocumentStore(StoreOptions options)
        {
            this.InnerFactory.ConfigureDocumentStore(options);

            options.AutoCreateSchemaObjects = AutoCreate.All;

            if (Debugger.IsAttached)
                options.Logger(new ConsoleMartenLogger());
        }
    }
}
