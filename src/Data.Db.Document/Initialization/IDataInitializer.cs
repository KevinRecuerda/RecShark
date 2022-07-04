using System.Threading.Tasks;

namespace RecShark.Data.Db.Document.Initialization
{
    using System.Threading;

    public interface IDataInitializer
    {
        Task Init(CancellationToken? cs = null);
    }
}
