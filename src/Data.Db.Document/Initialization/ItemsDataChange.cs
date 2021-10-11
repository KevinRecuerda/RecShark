using Marten;

namespace RecShark.Data.Db.Document.Initialization
{
    public abstract class ItemsDataChange : DataChange
    {
        public override void Run(IDocumentSession session, IDocumentStore store)
        {
            var items = this.BuildItems();
            session.Store(items);
        }

        public abstract object[] BuildItems();
    }
}