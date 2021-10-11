using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Npgsql;

namespace RecShark.Data.Db.Relational.Extensions
{
    public static class DbConnectionExtensions
    {
        private static readonly Regex SpecialConnections = new Regex(".*(testing|profiling).*");

        public static bool IsTestConnection(this IDbConnection connection)
        {
            var connectionNamespace = connection?.GetType().Namespace?.ToLower();
            if (connectionNamespace == null)
                return false;

            var isTestConnectionMatched = SpecialConnections.IsMatch(connectionNamespace);
            return isTestConnectionMatched;
        }

        public static async Task BulkInsert<T>(this IDbConnection connection, IEnumerable<T> items)
        {
            if (connection.IsTestConnection())
            {
                var innerConnection = (IDbConnection) connection.GetPropertyValue("InnerConnection")
                                   ?? (IDbConnection) connection.GetPropertyValue("WrappedConnection"); 
                if (innerConnection == null)
                    throw new InnerConnectionNotFound(connection);

                await innerConnection.BulkInsert(items);
                return;
            }

            if (!(connection is NpgsqlConnection npgsqlConnection))
                throw new NotSupportedException($"Bulk insert not supported for type {connection.GetType()}");

            npgsqlConnection.BulkInsert(items);
        }

        private static object GetPropertyValue(this object element, string propertyName)
        {
            var propertyInfo = element.GetType().GetProperties().FirstOrDefault(p => p.Name == propertyName);

            return propertyInfo != null ? propertyInfo.GetValue(element, null) : null;
        }
    }
}