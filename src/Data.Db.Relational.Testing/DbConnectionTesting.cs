using System;
using System.Data;
using System.Data.Common;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace RecShark.Data.Db.Relational.Testing
{
    public class DbConnectionTesting : IDbConnection
    {
        public static class ProfiledDbConnectionFactory
        {
            // Hack: to make postgresql works with Dapper & MiniProfiler
            // https://github.com/StackExchange/Dapper/issues/589
            // Also, see FeatureSupport: https://github.com/StackExchange/Dapper/blob/master/Dapper/FeatureSupport.cs
            public class NpgSqlConnection : ProfiledDbConnection
            {
                public NpgSqlConnection(DbConnection connection, IDbProfiler profiler) : base(connection, profiler) { }
            }

            public static IDbConnection CreateProfiledDbConnectionForDapper(IDbConnection connection)
            {
                return CreateProfiledDbConnectionForDapper((DbConnection) connection, MiniProfiler.Current);
            }

            public static IDbConnection CreateProfiledDbConnectionForDapper(DbConnection connection, IDbProfiler profiler)
            {
                var connectionType = connection?.GetType().Name;
                return string.Equals(connectionType, "npgsqlconnection", StringComparison.OrdinalIgnoreCase)
                           ? new NpgSqlConnection(connection, profiler)
                           : new ProfiledDbConnection(connection, profiler);
            }
        }

        public IDbConnection InnerConnection => dbConnection;

        public readonly  IDbConnection  dbConnection;
        private readonly IDbTransaction dbTransaction;

        public DbConnectionTesting(IDbConnection dbConnection)
        {
            this.dbConnection = ProfiledDbConnectionFactory.CreateProfiledDbConnectionForDapper(dbConnection);
            this.dbConnection.Open();

            dbTransaction = this.dbConnection.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void ForceDispose()
        {
            dbTransaction.Rollback();
            dbConnection.Dispose();
        }

        // Do nothing
        public void Dispose() { }

        public void Close() { }

        public void Open() { }

        public IDbTransaction BeginTransaction() => new DbTransactionTesting();

        public IDbTransaction BeginTransaction(IsolationLevel il) =>  new DbTransactionTesting();

        // Just call real connection
        public void ChangeDatabase(string databaseName) => dbConnection.ChangeDatabase(databaseName);

        public IDbCommand CreateCommand()
        {
            var command         = dbConnection.CreateCommand();
            command.Transaction = dbTransaction;
            return command;
        }

        public string ConnectionString
        {
            get => dbConnection.ConnectionString;
            set => dbConnection.ConnectionString = value;
        }

        public int ConnectionTimeout => dbConnection.ConnectionTimeout;

        public string Database => dbConnection.Database;

        public ConnectionState State => dbConnection.State;
    }

    public class DbTransactionTesting : IDbTransaction
    {
        public void Dispose() { }

        public void Commit() { }

        public void Rollback() { }

        public IDbConnection  Connection     { get; }
        public IsolationLevel IsolationLevel { get; }
    }
}