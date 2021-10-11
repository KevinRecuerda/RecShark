using System.Linq;
using Marten;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    public static class DocumentSessionExtensions
    {
        public static void RenameColumn<T>(this IDocumentSession session, string oldName, string newName)
        {
            var doc = session.DocumentStore.Tenancy.Default.MappingFor(typeof(T)).ToQueryableDocument();

            var query = $@"
update {doc.Table.Schema}.{doc.Table.Name}
set data = data - '{oldName}' || jsonb_build_object('{newName}', data->'{oldName}')
where data ? '{oldName}'
";
            session.Execute(query);
        }

        public static bool DoesTableExist(this IDocumentSession session, string table)
        {
            return session.Query<bool>($"select to_regclass('{table}') is not null").FirstOrDefault();
        }

        public static void Execute(this IDocumentSession session, string sql)
        {
            var command = session.Connection.CreateCommand();
            command.CommandText = sql;

            command.ExecuteNonQuery();
        }
    }
}