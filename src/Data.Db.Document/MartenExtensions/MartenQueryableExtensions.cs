using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Baseline;
using Marten;
using Marten.Linq.MatchesSql;
using Npgsql;
using Baseline.Reflection;
using Marten.Linq.Fields;
using Marten.Linq.Parsing;
using System.Collections;
using System.Reflection;
using RecShark.Extensions;
using Weasel.Postgresql;
using CommandExtensions = Weasel.Postgresql.CommandExtensions;

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

            var builder = new CommandBuilder();
            builder.Append(sql);

            // parameters
            foreach (var parameter in parameters)
            {
                if (parameter.IsAnonymousType())
                {
                    builder.AddParameters(parameter);

                    //command.AddParameters(parameter);
                }
                else
                {
                    //TODO: check if need to call useParameter
                    var npgParameter = builder.AddParameter(parameter);

                    // var npgParameter = command.AddParameter(parameter);
                    // sql.UseParameter(npgParameter);
                }
            }

            //command.CommandText = sql;
            return RunCommand<TOut>(session, builder.Compile());
        }

        public static IReadOnlyList<T> WhereArray<T, TArray, TFilter>(
            this IQueryable<T>                       source,
            IDocumentSession                         session,
            Expression<Func<T, IEnumerable<TArray>>> arraySelector,
            Expression<Func<TArray, TFilter>>        filterSelector,
            TFilter[]                                parameters,
            bool                                     useWildcard = false)
        {
            if (parameters == null || parameters.Length == 0)
                return source.ToList();

            var filterOperator = "=";
            if (useWildcard && typeof(TFilter) == typeof(string))
            {
                filterOperator = "ilike";
                parameters     = parameters.Cast<string>().Select(x => x.Replace("*", "%")).Cast<TFilter>().ToArray();
            }

            var arrayCol  = SelectorToFieldLight(session, arraySelector);
            var filterCol = SelectorToFieldLight(session, filterSelector);

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
            CommandExtensions.AddNamedParameter(command, "arrayParams", parameters);
            command.CommandText = sql;

            return RunCommand<T>(session, command);
        }

        public static IQueryable<TSource> Where<TSource, TInclude>(
            this IQueryable<TSource>         source,
            Expression<Func<TInclude, bool>> predicate,
            IQuerySession                    session,
            int                              includeIndex = 0)
        {
            const string includeTableAliasPrefix = "incl";
            const string martenDefaultAlias      = "d.";

            var (includeTable, includeId, condition, parameters) = GetEntityNames<TInclude>(session, predicate);
            var (sourceTable, sourceId, _, _)                    = GetEntityNames<TSource>(session);
            var includes = GetIncludes(source);

            //TODO: check if includes are always in same order
            var joins = "";
            foreach (var (include, i) in includes.Select((v, i) => (v, i)))
            {
                var connectingField = GetConnectingField(include);
                var locator         = connectingField.TypedLocator.Replace(martenDefaultAlias, "src.");
                joins += $" INNER JOIN {includeTable} as {includeTableAliasPrefix}{i} on {locator} = {includeTableAliasPrefix}{i}.{includeId}";
            }

            condition = condition.Replace(martenDefaultAlias, $"{includeTableAliasPrefix}{includeIndex}.");
            condition = Regex.Replace(condition, @":p(\d+)", "?");

            // TODO: check if there is a limit on number of elements of "in" list
            var matchSql = @$"{sourceId} in (
select src.{sourceId} 
from {sourceTable} as src {joins} 
where {condition}
)";
            return source.Where(x => x.MatchesSql(matchSql, parameters));
        }

        private static IField GetConnectingField(object include)
        {
            return (IField) include?.GetType().GetProperty("ConnectingField").GetValue(include);
        }

        private static IEnumerable<object> GetIncludes(IQueryable source)
        {
            var provider = source.GetType().GetProperty("MartenProvider").GetValue(source);
            var includes = (ICollection) provider.GetType()
                                                 .GetProperty("AllIncludes", BindingFlags.NonPublic|BindingFlags.Instance)
                                                 .GetValue(provider);
            return includes.Cast<object>();
        }

        private static (string tableName, string idName, string whereClause, object[] parameters) GetEntityNames<T>(
            IQuerySession             session,
            Expression<Func<T, bool>> predicate = null)
        {
            // select d.id, d.data from document_store_tests.mt_doc_control as d where CAST(d.data ->> 'Result' as integer) = :p0
            IQueryable<T> query = session.Query<T>();
            if (predicate != null)
                query = query.Where(predicate);

            var command    = query.ToCommand();
            var parameters = command.Parameters.Select(p => p.Value).ToArray();

            var commandText    = command.CommandText;
            var indexStartFrom = commandText.IndexOf("from ") + 5;
            var indexEndFrom   = commandText.IndexOf(" as d");

            var tableName      = commandText.Substring(indexStartFrom, indexEndFrom             - indexStartFrom);
            var idColumn       = commandText.Substring(9,              commandText.IndexOf(",") - 9);
            var whereCondition = commandText.Substring(commandText.IndexOf("where ")            + 5);

            return (tableName, idColumn, whereCondition, parameters);
        }

        //TODO: implement Latest
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

            var includes = GetIncludes(source);

            var aliases = new List<string>();
            foreach (var include in includes)
            {
                aliases.Add((string) include.GetType().GetProperty("TempTableSelector").GetValue(include));
            }

            object myInclude = null;
            foreach (var include in includes)
            {
                if (include.GetType().GetProperty("DocumentType").GetValue(include) == typeof(TInclude))
                {
                    myInclude = include;
                    break;
                }
            }

            var includeAlias = (string) myInclude?.GetType().GetProperty("TempTableSelector").GetValue(myInclude);

            var groupBy = groupBySelectors.Select(g => SelectorToSqlSafely(session, g, includeAlias)).Join(",");
            var max     = SelectorToSqlSafely(session, maxSelector, includeAlias);

            var query            = command.CommandText;
            var indexOfFirstFrom = query.IndexOf("from ");
            var initialQuery     = query.Substring(indexOfFirstFrom);
            if (aliases.Any())
            {
                // when there is no includes query is just a select ...
                // in case there are includes only ?
                var endOfFirstQuery = query.IndexOf(");");
                initialQuery = query.Substring(indexOfFirstFrom, endOfFirstQuery - indexOfFirstFrom);
            }

            //TODO manage group by on an Included object !
            // Marten does not generates the join query => can't group on included field

            query = $"SELECT {groupBy}, max({max}) {initialQuery} GROUP BY {groupBy}";
            query = Regex.Replace(query, @":arg(\d+)", "?");

            // var aliases = includes.Select(i => i.TableAlias).ToList();
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

            // TODO: field.MemberType -> FieldType ?
            return field.FieldType.IsNullableType() ? $"COALESCE({sqlSelector},'')" : sqlSelector;
        }

        // TODO: find a way to access MappingFor<> , document Mappings
        private static string SelectorToFieldLight<T, TResult>(IDocumentSession session, Expression<Func<T, TResult>> selector)
        {
            var findMembers = new FindMembers();
            findMembers.Visit(selector);
            var members = findMembers.Members.ToArray();
            return members.Single().Name;
        }

        private static IField SelectorToField<T, TResult>(IDocumentSession session, Expression<Func<T, TResult>> selector)
        {
            var findMembers = new FindMembers();
            findMembers.Visit(selector);
            var members = findMembers.Members.ToArray();

            //TODO check if ok
            var docProvider = session.Database.Providers.StorageFor<T>();
            var field       = docProvider.Lightweight.Fields.FieldFor(members);
            return field;

            //var mapping     = ((QuerySession) session.DocumentStore.QuerySession()).StorageFor(typeof(T));
            //var mapping = ((DocumentStore) session.DocumentStore).Options.Storage.MappingFor(typeof(T));
            //var field = mapping.Fields.FieldFor(members);
            //return field;
        }

        private static IReadOnlyList<T> RunCommand<T>(IDocumentSession session, NpgsqlCommand command)
        {
            command.Connection = session.Connection;

            var reader = command.ExecuteReader();

            var serializer = (session.DocumentStore as DocumentStore).Serializer;

            var results = new List<T>();
            while (reader.Read())
            {
                var result = serializer.FromJson<T>(reader.GetStream(0));
                results.Add(result);
            }

            return results;
        }

        // public async Task<TDataReader> ExecuteReaderAsync(TConnection       conn,
        //                                                   CancellationToken cancellation = default, TTransaction? tx = null)
        // {
        //     var cmd = Compile();
        //     cmd.Connection  = conn;
        //     cmd.Transaction = tx;
        //
        //     return (TDataReader) await cmd.ExecuteReaderAsync(cancellation).ConfigureAwait(false);
        // }
        //
        // public async Task<IReadOnlyList<T>> FetchList<T>(TConnection       conn,                   Func<DbDataReader, Task<T>> transform,
        //                                                  CancellationToken cancellation = default, TTransaction?               tx = null)
        // {
        //     var cmd = Compile();
        //     cmd.Connection  = conn;
        //     cmd.Transaction = tx;
        //
        //     var list = new List<T>();
        //
        //     using var reader = await cmd.ExecuteReaderAsync(cancellation).ConfigureAwait(false);
        //     while (await reader.ReadAsync(cancellation).ConfigureAwait(false))
        //     {
        //         list.Add(await transform(reader).ConfigureAwait(false));
        //     }
        //
        //     return list;
        // }        
    }
}
