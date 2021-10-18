using Marten.Schema;
using Marten.Storage;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    public class SimpleIndex : IIndexDefinition
    {
        public SimpleIndex(IQueryableDocument mapping, string shortName, string column, IndexMethod method, string op = "")
        {
            IndexName = $"{mapping.Table.Name}_idx_{shortName}";
            Table     = mapping.Table;
            Column    = column;
            Method    = method;
            Op        = op;
        }

        public string ToDDL()
        {
            return $"CREATE INDEX {IndexName} ON {Table.QualifiedName} USING {Method} (({Column}) {Op});";
        }

        public bool Matches(ActualIndex index)
        {
            return index != null;
        }

        public string       IndexName { get; }
        public DbObjectName Table     { get; }
        public string       Column    { get; }
        public IndexMethod  Method    { get; }
        public string       Op        { get; }
    }
}
