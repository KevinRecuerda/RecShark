using Marten;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Data.Db.Document.Initialization;
using RecShark.Extensions.DependencyInjection;

namespace RecShark.Data.Db.Document
{
    public class DocumentStoreDataModule<TFactory, TConnection> : DIModule
        where TFactory : BaseDocumentStoreFactory
        where TConnection : class, IConnectionString
    {
        public override void Load(IServiceCollection services)
        {
            services.AddSingleton<IConnectionString, TConnection>();
            services.AddSingleton<IDocumentStoreFactory, TFactory>();
            services.AddSingleton<IDocumentStore>(s => s.GetService<IDocumentStoreFactory>().CreateDocumentStore());

            services.AddTransient<IDataInitializer, DataInitializer>();
            services.AddTransient<IDataLocker, DataLocker>();
        }
    }
}