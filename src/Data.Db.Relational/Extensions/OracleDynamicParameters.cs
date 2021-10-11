using System.Collections.Generic;
using System.Data;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace RecShark.Data.Db.Relational.Extensions
{
    public class OracleDynamicParameters : SqlMapper.IDynamicParameters
    {
        private readonly DynamicParameters     dynamicParameters = new DynamicParameters();
        private readonly List<OracleParameter> oracleParameters  = new List<OracleParameter>();

        public void Add(string name, object value = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null)
        {
            this.dynamicParameters.Add(name, value, dbType, direction, size);
        }

        public void Add(string name, OracleDbType oracleDbType, ParameterDirection direction)
        {
            var oracleParameter = new OracleParameter(name, oracleDbType, direction);
            this.oracleParameters.Add(oracleParameter);
        }

        public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            ((SqlMapper.IDynamicParameters) this.dynamicParameters).AddParameters(command, identity);

            var oracleCommand = command as OracleCommand;
            oracleCommand?.Parameters.AddRange(this.oracleParameters.ToArray());
        }
    }
}