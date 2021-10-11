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
            this.InnerFactory = innerFactory;
        }

        public IDbConnectionFactory InnerFactory { get; }

        public IDbConnection CreateDbConnection(string type, string name)
        {
            if (!this.testingConnections.ContainsKey(name))
            {
                var innerConnection = this.InnerFactory.CreateDbConnection(type, name);
                this.testingConnections[name] = new DbConnectionTesting(innerConnection);
            }

            return this.testingConnections[name];
        }

        public SqlDialect GetSqlDialect(string type) => this.InnerFactory.GetSqlDialect(type);

        public void Dispose()
        {
            // Rollback transaction
            foreach (var testingConnection in this.testingConnections.Values)
                testingConnection.ForceDispose();
        }
    }
}