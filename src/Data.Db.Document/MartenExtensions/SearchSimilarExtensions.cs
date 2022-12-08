using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Marten.Internal.Sessions;
using Marten.Linq.Parsing;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    public static class SearchSimilarExtensions
    {
        // TODO: manage Includes
        public static async Task<IReadOnlyList<T>> SearchSimilarAsync<T>(
            this IQuerySession          session,
            Expression<Func<T, string>> selector,
            string                      search,
            int                         top)
        {
            var members = FindMembers.Determine(selector);

            var doc = ((QuerySession) session.DocumentStore.QuerySession()).StorageFor(typeof(T)).Fields;

            //var doc     = session.DocumentStore.Tenancy.Default.MappingFor(typeof(T)).ToQueryableDocument();
            var locator = doc.FieldFor(members).RawLocator;

            var sql = " where 1=1";
            sql += $" order by {locator} <-> :{nameof(search)}";
            sql += $" limit :{nameof(top)}";

            return await session.QueryAsync<T>(
                       sql,
                       new CancellationToken(),
                       new
                       {
                           search,
                           top
                       });
        }
    }
}
