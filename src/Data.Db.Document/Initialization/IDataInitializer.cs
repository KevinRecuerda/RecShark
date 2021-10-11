using System.Threading.Tasks;

namespace RecShark.Data.Db.Document.Initialization
{
    public interface IDataInitializer
    {
        Task Init();
    }
}