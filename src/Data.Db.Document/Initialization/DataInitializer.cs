using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.Logging;

namespace RecShark.Data.Db.Document.Initialization
{
    public class DataInitializer : IDataInitializer
    {
        private readonly ILogger               logger;
        private readonly IDocumentStoreFactory factory;

        public DataInitializer(IDocumentStoreFactory factory, ILogger<DataInitializer> logger)
        {
            this.factory = factory;
            this.logger  = logger;
        }

        public async Task Init()
        {
            this.logger.LogInformation("Initializing store for schema {schema} ...", this.factory.Schema);
            var store = this.factory.CreateDocumentStore();

            var locker = this.CreateDataLocker(store);
            this.logger.LogInformation("awaiting lock ...");
            await locker.AcquireLock();

            try
            {
                await this.ApplyChanges(store, this.factory.DataChanges, ExecutionMode.PreSchemaChanges);

                this.logger.LogInformation("applying auto schema change ...");
                store.Schema.ApplyAllConfiguredChangesToDatabase();

                await this.ApplyChanges(store, this.factory.DataChanges, ExecutionMode.PostSchemaChanges);
            }
            catch (Exception exception)
            {
                var message = exception is MartenSchemaException && exception.InnerException != null
                                  ? exception.InnerException.Message
                                  : exception.Message;
                this.logger.LogError(exception, $"error occured: {message}");
            }
            finally
            {
                this.logger.LogInformation("releasing lock ...");
                await locker.ReleaseLock();
            }
        }

        public virtual async Task ApplyChanges(IDocumentStore store, DataChange[] dataChanges, ExecutionMode executionMode)
        {
            var changesToExecute = await this.GetChangesToExecute(store, dataChanges, executionMode);

            foreach (var change in changesToExecute)
            {
                this.logger.LogInformation("applying change {changeId} ...", change.Id);
                using (var session = store.OpenSession())
                {
                    change.Run(session, store);
                    session.Store(change.ToLog());
                    session.SaveChanges();
                }
            }
        }

        public async Task<DataChange[]> GetChangesToExecute(IDocumentStore store, DataChange[] dataChanges, ExecutionMode executionMode)
        {
            var dataChangesMatchingMode = dataChanges.Where(d => d.ExecutionMode == executionMode).ToArray();
            if (dataChangesMatchingMode.IsEmpty())
                return dataChangesMatchingMode;

            var historicalChanges = (await GetDataChanges(store)).ToDictionary(x => x.Id);

            return dataChangesMatchingMode.Where(x => this.MustExecuteChange(x, historicalChanges.GetValueOrDefault(x.Id)))
                                          .ToArray();
        }

        public static async Task<IReadOnlyList<DataChangeLog>> GetDataChanges(IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                return await session.Query<DataChangeLog>().ToListAsync();
            }
        }

        public virtual bool MustExecuteChange(IDataChange change, IDataChange historicalChange)
        {
            return change.RunAlways
                || historicalChange == null
                || change.IsMoreRecent(historicalChange);
        }

        public virtual IDataLocker CreateDataLocker(IDocumentStore store)
        {
            var locker = new DataLocker(store);
            return locker;
        }
    }
}
