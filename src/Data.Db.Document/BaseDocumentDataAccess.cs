using Marten;

namespace RecShark.Data.Db.Document
{
    public abstract class BaseDocumentDataAccess
    {
        protected readonly IDocumentStore DocumentStore;

        protected BaseDocumentDataAccess(IDocumentStore documentStore)
        {
            this.DocumentStore = documentStore;
        }
    }
}