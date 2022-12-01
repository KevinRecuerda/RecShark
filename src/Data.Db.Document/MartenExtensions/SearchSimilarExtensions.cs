using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Marten.Linq;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    using Marten.Linq.Parsing;

    public static class SearchSimilarExtensions
    {
        public static async Task<IReadOnlyList<T>> SearchSimilarAsync<T>(
            this IQuerySession          session,
            Expression<Func<T, string>> selector,
            string                      search,
            int                         top)
        {
            var members = FindMembers.Determine(selector);
            var doc     = session.DocumentStore.Tenancy.Default.MappingFor(typeof(T)).ToQueryableDocument();
            var locator = doc.FieldFor(members).SqlLocator;

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
