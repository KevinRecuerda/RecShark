using System.Linq;
using Marten;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    using Marten.Internal.Sessions;

    public static class DocumentSessionExtensions
    {
        public static void RenameColumn<T>(this IDocumentSession session, string oldName, string newName)
        {
            var doc = ((QuerySession) session.DocumentStore.QuerySession()).StorageFor(typeof(T));

            var query = $@"
update {doc.TableName.Schema}.{doc.TableName.Name}
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
