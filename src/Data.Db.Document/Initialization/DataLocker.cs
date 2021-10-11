using System;
using System.Threading;
using System.Threading.Tasks;
using Marten;

namespace RecShark.Data.Db.Document.Initialization
{
    public class DataLocker : BaseDocumentDataAccess, IDataLocker
    {
        public int SleepTime { get; set; } = 10000;
        public int MaxRetry  { get; set; } = 6;

        public DataLocker(IDocumentStore documentStore) : base(documentStore) { }

        public async Task AcquireLock()
        {
            for (var retry = 0; retry < this.MaxRetry; retry++)
            {
                if (await this.TryLock())
                    return;

                Thread.Sleep(this.SleepTime);
            }

            throw new DataLockException(this.MaxRetry);
        }

        public async Task ReleaseLock()
        {
            using (var session = this.DocumentStore.OpenSession())
            {
                session.DeleteWhere<DataLock>(x => x.Host == Environment.MachineName);
                await session.SaveChangesAsync();
            }
        }

        public async Task<bool> TryLock()
        {
            try
            {
                var @lock = new DataLock {Host = Environment.MachineName};

                using (var session = this.DocumentStore.OpenSession())
                {
                    session.Insert(@lock);
                    await session.SaveChangesAsync();
                }

                return true;
            }
            catch (Marten.Exceptions.MartenCommandException e) when (e.Message.Contains("duplicate key value violates"))
            {
                return false;
            }
        }

        public async Task<DataLock> GetLock()
        {
            using (var session = this.DocumentStore.OpenSession())
            {
                return await session.Query<DataLock>().SingleOrDefaultAsync();
            }
        }
    }
}