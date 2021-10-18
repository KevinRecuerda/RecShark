using System;
using System.Collections.Generic;
using System.Data;
using Dapper.FastCrud;

namespace RecShark.Data.Db.Relational.Testing
{
    public class DbConnectionFactoryTesting : IDbConnectionFactory, IDisposable
    {
        private readonly Dictionary<string, DbConnectionTesting> testingConnections = new Dictionary<string, DbConnectionTesting>();

        public DbConnectionFactoryTesting(IDbConnectionFactory innerFactory)
        {
            InnerFactory = innerFactory;
        }

        public IDbConnectionFactory InnerFactory { get; }

        public IDbConnection CreateDbConnection(string type, string name)
        {
            if (!testingConnections.ContainsKey(name))
            {
                var innerConnection = InnerFactory.CreateDbConnection(type, name);
                testingConnections[name] = new DbConnectionTesting(innerConnection);
            }

            return testingConnections[name];
        }

        public SqlDialect GetSqlDialect(string type) => InnerFactory.GetSqlDialect(type);

        public void Dispose()
        {
            // Rollback transaction
            foreach (var testingConnection in testingConnections.Values)
                testingConnection.ForceDispose();
        }
    }
}