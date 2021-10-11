using Marten;
using RecShark.Data.Db.Document.Initialization;

namespace RecShark.Data.Db.Document
{
    public interface IDocumentStoreFactory
    {
        string Schema { get; }

        DataChange[] DataChanges { get; }

        IDocumentStore CreateDocumentStore();
    }
}