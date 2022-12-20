using System;
using System.Threading;
using System.Threading.Tasks;
using Marten;

namespace RecShark.Data.Db.Document.Initialization
{
    using Marten.Linq.LastModified;

    public class DataLocker : BaseDocumentDataAccess, IDataLocker
    {
        public int      SleepTime   { get; set; } = 10000;
        public int      MaxRetry    { get; set; } = 6;
        public TimeSpan MaxLockTime { get; set; } = TimeSpan.FromHours(4);

        public DataLocker(IDocumentStore documentStore) : base(documentStore) { }

        public async Task AcquireLock()
        {
            for (var retry = 0; retry < MaxRetry; retry++)
            {
                if (await TryLock())
                    return;

                Thread.Sleep(SleepTime);
            }

            throw new DataLockException(MaxRetry);
        }

        public async Task ReleaseLock()
        {
            using (var session = DocumentStore.OpenSession())
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

                using (var session = DocumentStore.OpenSession())
                {
                    session.DeleteWhere<DataLock>(l => l.ModifiedBefore(DateTimeOffset.Now.UtcDateTime.Subtract(this.MaxLockTime)));
                    session.Insert(@lock);
                    await session.SaveChangesAsync();
                }

                return true;
            }
            catch (Marten.Exceptions.DocumentAlreadyExistsException)
            {
                return false;
            }
        }

        public async Task<DataLock> GetLock()
        {
            using (var session = DocumentStore.OpenSession())
            {
                return await session.Query<DataLock>().SingleOrDefaultAsync();
            }
        }
    }
}
