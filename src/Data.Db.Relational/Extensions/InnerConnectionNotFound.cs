using System;
using System.Data;

namespace RecShark.Data.Db.Relational.Extensions
{
    public class InnerConnectionNotFound : Exception
    {
        public InnerConnectionNotFound(IDbConnection connection)
            : base($"Could not get InnerConnection from {connection.GetType()}") { }
    }
}