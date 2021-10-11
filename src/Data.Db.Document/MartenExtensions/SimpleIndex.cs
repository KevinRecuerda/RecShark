using Marten.Schema;
using Marten.Storage;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    public class SimpleIndex : IIndexDefinition
    {
        public SimpleIndex(IQueryableDocument mapping, string shortName, string column, IndexMethod method, string op = "")
        {
            this.IndexName = $"{mapping.Table.Name}_idx_{shortName}";
            this.Table     = mapping.Table;
            this.Column    = column;
            this.Method    = method;
            this.Op        = op;
        }

        public string ToDDL()
        {
            return $"CREATE INDEX {this.IndexName} ON {this.Table.QualifiedName} USING {this.Method} (({this.Column}) {this.Op});";
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
