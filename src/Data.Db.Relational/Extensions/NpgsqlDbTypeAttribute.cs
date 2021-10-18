using System;
using NpgsqlTypes;

namespace RecShark.Data.Db.Relational.Extensions
{
    public class NpgsqlDbTypeAttribute : Attribute
    {
        public NpgsqlDbTypeAttribute(NpgsqlDbType npgsqlType)
        {
            NpgsqlType = npgsqlType;
        }

        public NpgsqlDbType NpgsqlType { get; }
    }
}