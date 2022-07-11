using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Baseline;
using Marten;
using Marten.Linq;
using Marten.Linq.MatchesSql;
using Marten.Schema;
using Marten.Services.Includes;
using Marten.Util;
using Npgsql;
using RecShark.Extensions;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    public static class MartenQueryableExtensions
    {
        public static IReadOnlyList<TOut> SelectFields<TIn, TOut>(
            this IQueryable<TIn> queryable,
            IDocumentSession     session,
            (string, string)[]   fields,
            params object[]      parameters)
        {
            var command = queryable.Explain().Command;

            // sql
            var jsonFields = string.Join(",", fields.Select(f => $"'{f.Item1}', {f.Item2}"));
            var baseSql    = command.CommandText.Substring(command.CommandText.IndexOf("from"));
            var sql        = $"select json_build_object({jsonFields}) as data {baseSql}";

            // parameters
            foreach (var parameter in parameters)
            {
                if (parameter.IsAnonymousType())
                    command.AddParameters(parameter);
                else
                {
                    var npgParameter = command.AddParameter(parameter);
                    sql.UseParameter(npgParameter);
                }
            }

            command.CommandText = sql;

            return RunCommand<TOut>(session, command);
        }

        public static IReadOnlyList<T> WhereArray<T, TArray, TFilter>(
            this IQueryable<T>                       source,
            IDocumentSession                         session,
            Expression<Func<T, IEnumerable<TArray>>> arraySelector,
            Expression<Func<TArray, TFilter>>        filterSelector,
            TFilter[]                                parameters)
        {
            return WhereArray(source, session, arraySelector, filterSelector, "=", parameters);
        }

        public static IReadOnlyList<T> WhereLikeArray<T, TArray>(
            this IQueryable<T>                       source,
            IDocumentSession                         session,
            Expression<Func<T, IEnumerable<TArray>>> arraySelector,
            Expression<Func<TArray, string>>         filterSelector,
            string[]                                 patterns)
        {
            var likePatterns = patterns.Select(x => x.Replace("*", "%")).ToArray();
            return WhereArray(source, session, arraySelector, filterSelector, "like", likePatterns);
        }

        private static IReadOnlyList<T> WhereArray<T, TArray, TFilter>(
            this IQueryable<T>                       source,
            IDocumentSession                         session,
            Expression<Func<T, IEnumerable<TArray>>> arraySelector,
            Expression<Func<TArray, TFilter>>        filterSelector,
            string                                   filterOperator,
            TFilter[]                                parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return source.ToList();

            var arrayCol  = SelectorToField(session, arraySelector).MemberName;
            var filterCol = SelectorToField(session, filterSelector).MemberName;

            // sql
            var pivot = $@"
select arr.* from jsonb_array_elements(d.data -> '{arrayCol}') arr
where arr ->> '{filterCol}' {filterOperator} ANY (:arrayParams)
";

            var command = source.Explain().Command;
            var baseSql = command.CommandText.Substring(command.CommandText.IndexOf("from"));
            var sql     = $"select d.data || jsonb_build_object('{arrayCol}', array({pivot})) as data {baseSql}";

            sql = $@"
with cte as ({sql})
select *
from cte
where cte.data -> '{arrayCol}' != '[]'::jsonb
";

            // parameters
            command.AddNamedParameter("arrayParams", parameters);
            command.CommandText = sql;

            return RunCommand<T>(session, command);
        }

        public static IQueryable<TSource> Where<TSource, TInclude>(
            this IQueryable<TSource>         source,
            Expression<Func<TInclude, bool>> predicate,
            IQuerySession                    session,
            int                              includeIndex = 0)
        {
            var command    = session.Query<TInclude>().Where(predicate).ToCommand();
            var parameters = command.Parameters.Select(p => p.Value).ToArray();

            var query       = command.CommandText;
            var whereClause = query.Substring(query.IndexOf("where ") + 5);

            var executor = source.As<MartenQueryable<TSource>>().Executor;
            var includes = executor.Includes.OfType<IncludeJoin<TInclude>>().ToArray();
            var include  = includes[includeIndex];

            whereClause = whereClause.Replace("d.", include.TableAlias + ".");
            whereClause = Regex.Replace(whereClause, @":arg(\d+)", "?");

            return source.Where(x => x.MatchesSql(whereClause, parameters));
        }

        public static IQueryable<T> Latest<T>(
            this IQueryable<T>                   source,
            IDocumentSession                     session,
            Expression<Func<T, object>>          maxSelector,
            params Expression<Func<T, object>>[] groupBySelectors)
        {
            return source.Latest<T, T>(session, maxSelector, groupBySelectors);
        }

        public static IQueryable<TSource> Latest<TSource, TInclude>(
            this IQueryable<TSource>                    source,
            IDocumentSession                            session,
            Expression<Func<TInclude, object>>          maxSelector,
            params Expression<Func<TInclude, object>>[] groupBySelectors)
        {
            var command    = source.ToCommand();
            var parameters = command.Parameters.Select(p => p.Value).ToArray();

            var includes     = source.As<MartenQueryable<TSource>>().Executor.Includes.ToList();
            var includeAlias = includes.OfType<IncludeJoin<TInclude>>().FirstOrDefault()?.TableAlias;

            var groupBy = groupBySelectors.Select(g => SelectorToSqlSafely(session, g, includeAlias)).Join(",");
            var max     = SelectorToSqlSafely(session, maxSelector, includeAlias);

            var query = command.CommandText;
            query = $"SELECT {groupBy}, max({max}) {query.Substring(query.IndexOf("from "))} GROUP BY {groupBy}";
            query = Regex.Replace(query, @":arg(\d+)", "?");

            var aliases = includes.Select(i => i.TableAlias).ToList();
            aliases.Insert(0, "d");

            foreach (var alias in aliases)
            {
                var latestAlias = $"{alias}_latest";
                query = query.Replace($" {alias}.", $" {latestAlias}.");
                query = query.Replace($",{alias}.", $",{latestAlias}.");
                query = query.Replace($"({alias}.", $"({latestAlias}.");
                query = query.Replace($" {alias} ", $" {latestAlias} ");
            }

            var whereClause = $"({groupBy}, {max}) in ({query})";
            return source.Where(x => x.MatchesSql(whereClause, parameters));
        }

        private static string SelectorToSqlSafely<T, TResult>(IDocumentSession session, Expression<Func<T, TResult>> selector, string alias = null)
        {
            alias ??= "d";

            var field       = SelectorToField(session, selector);
            var sqlSelector = field.LocatorFor(alias);
            return field.MemberType.IsNullableType() ? $"COALESCE({sqlSelector},'')" : sqlSelector;
        }

        private static IField SelectorToField<T, TResult>(IDocumentSession session, Expression<Func<T, TResult>> selector)
        {
            var findMembers = new FindMembers();
            findMembers.Visit(selector);
            var members = findMembers.Members.ToArray();

            var mapping = ((DocumentStore) session.DocumentStore).Options.Storage.MappingFor(typeof(T));
            var field   = mapping.FieldFor(members);
            return field;
        }

        private static IReadOnlyList<T> RunCommand<T>(IDocumentSession session, NpgsqlCommand command)
        {
            var reader = command.ExecuteReader();

            var serializer = (session.DocumentStore as DocumentStore).Serializer;

            var results = new List<T>();
            while (reader.Read())
            {
                var result = serializer.FromJson<T>(reader.GetTextReader(0));
                results.Add(result);
            }

            return results;
        }
    }
}
