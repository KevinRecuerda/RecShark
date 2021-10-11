using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace RecShark.Data.Db.Relational.Tests
{
    public interface ISizeDataAccess
    {
        Task<ICollection<SizeObject>> GetSchemaSize();
    }

    public class SizeDataAccess : SampleBaseDataAccess, ISizeDataAccess
    {
        public SizeDataAccess(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<ICollection<SizeObject>> GetSchemaSize()
        {
            const string query = @"
select 
	schemaname as id, 
	pg_size_pretty(SUM(pg_total_relation_size(fullname)::BIGINT)) as size
from (
	select schemaname, quote_ident(schemaname) || '.' || quote_ident(tablename) as fullname
	from pg_tables
) n
group by schemaname";
            using (var db = this.Open())
            {
                var sizes = await db.QueryAsync<SizeObject>(query);
                return sizes.ToList();
            }
        }
    }

    public class SizeObject
    {
        public string Id   { get; set; }
        public string Size { get; set; }
    }
}