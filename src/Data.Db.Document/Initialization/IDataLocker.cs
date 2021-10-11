using System.Threading.Tasks;

namespace RecShark.Data.Db.Document.Initialization
{
    public interface IDataLocker
    {
        Task AcquireLock();
        Task ReleaseLock();
    }
}