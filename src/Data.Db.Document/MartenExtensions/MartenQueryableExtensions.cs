﻿using System;
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

namespace RecShark.Data.Db.Document.MartenExtensions
{
    using Marten.Internal.Storage;

    public static class MartenQueryableExtensions
    {
        public static IReadOnlyList<TOut> SelectFields<TIn, TOut>(
            this IQueryable<TIn> queryable,
            IDocumentSession     session,
            (string, string)[]   fields,
            params object[]      parameters)
        {
            const string includeTableAlias = "sfin";
            
            var          command           = queryable.ToCommand();
            var          jsonFields        = string.Join(",", fields.Select(f => $"'{f.Item1}', {f.Item2}"));

            var          includes          = GetIncludes(queryable);
            
            var          joins             = "";
            foreach (var (include, i) in includes.Select((v, i) => (v, i)))
            {
                var connectingField = GetProp<IField>(include, "ConnectingField");
                var locator         = connectingField.TypedLocator;
                var storage         = GetStorage(include);
                var table           = storage.TableName;

                //TODO: check if may need to OUTER JOIN
                const string includeId = "id"; //TODO: find id column selector
                joins += $" INNER JOIN {table.QualifiedName} as {includeTableAlias}{i} on {locator} = {includeTableAlias}{i}.{includeId}";
            }

            var commandText = command.CommandText;
            if (commandText.Contains("create temp table"))
            {
                var indexOfSelect   = commandText.IndexOf("select");
                var indexOfEndQuery = commandText.IndexOf(");");
                commandText = commandText.Substring(indexOfSelect, indexOfEndQuery - indexOfSelect);
                commandText = commandText.Replace(" as d ", $" as d {joins}");
            }

            var indexOfFrom = commandText.IndexOf("from");
            var baseSql     = commandText.Substring(indexOfFrom);
            var sql         = $"select json_build_object({jsonFields}) as data {baseSql}";

            var builder = new CommandBuilder(command);
            foreach (var parameter in parameters)
            {
                if (parameter.IsAnonymousType())
                    builder.AddParameters(parameter);
                else
                    builder.AddParameter(parameter);
            }
            command.CommandText = sql;
            return RunCommand<TOut>(session, command);
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
where cte.data -> '{arrayCol}' != '[]'::jsonb";

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
            const string includeTableAliasPrefix = "incl";
            const string martenDefaultAlias      = "d.";

            var (_, _, condition, parameters) = GetEntityNames<TInclude>(session, predicate);
            var (sourceTable, sourceId, _, _)                    = GetEntityNames<TSource>(session);
            var includes = GetIncludes(source);

            //TODO: check if includes are always in same order
            //TODO: manage includes on multiple types ? => table name won't be the same
            //TODO: test multiple Where conditions
            var joins = "";
            foreach (var (include, i) in includes.Select((v, i) => (v, i)))
            {
                var          connectingField = GetProp<IField>(include, "ConnectingField");
                var          locator         = connectingField.TypedLocator.Replace(martenDefaultAlias, "src.");
                var          storage         = GetStorage(include);
                var          table           = storage.TableName;
                const string id              = "id"; //TODO: need to get this name dynamically
                
                //TODO: check if may need to OUTER JOIN
                joins += $" INNER JOIN {table} as {includeTableAliasPrefix}{i} on {locator} = {includeTableAliasPrefix}{i}.{id}";
            }

            condition = condition.Replace(martenDefaultAlias, $"{includeTableAliasPrefix}{includeIndex}.");
            condition = Regex.Replace(condition, @":p(\d+)", "?");

            // TODO: check if there is a limit on number of elements of "in" list
            var matchSql = @$"d.{sourceId} in (
select src.{sourceId} 
from {sourceTable} as src {joins} 
where {condition}
)";

            var queryable = source.Where(x => x.MatchesSql(matchSql, parameters));
            return queryable;
        }

        // called with IncludePlan
        private static T GetProp<T>(object includePlan, string propName)
        {
            return (T) includePlan?.GetType().GetProperty(propName).GetValue(includePlan);
        }

        private static IEnumerable<object> GetIncludes(IQueryable source)
        {
            var provider = source.GetType().GetProperty("MartenProvider").GetValue(source);
            var includes = (ICollection) provider.GetType()
                                                 .GetProperty("AllIncludes", BindingFlags.NonPublic|BindingFlags.Instance)
                                                 .GetValue(provider);
            return includes.Cast<object>();
        }

        private static IDocumentStorage GetStorage(object includePlan)
        {
            return (IDocumentStorage) includePlan?.GetType().GetField("_storage", BindingFlags.NonPublic|BindingFlags.Instance).GetValue(includePlan);
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
            const string martenDefaultAlias = "d.";
            const string grpAlias           = "grp";
            const string srcLatestAlias     = "srclast";
            const string incAlias           = "inclast";

            var command    = source.ToCommand();
            var parameters = command.Parameters.Select(p => p.Value).ToArray();

            var (sourceTable, _, _, _)          = GetEntityNames<TSource>(session);
            var (includeTable, includeId, _, _) = GetEntityNames<TInclude>(session);

            var includes  = GetIncludes(source);
            var myInclude = includes.FirstOrDefault(i => i.GetType().GetProperty("DocumentType").GetValue(i) == typeof(TInclude));

            // connectorField.TypedLocator contains d. prefix
            var connectorField = GetProp<IField>(myInclude, "ConnectingField");

            var groupBy = groupBySelectors.Select(g => SelectorToSqlSafely(session, g, grpAlias)).Join(",");
            var max     = SelectorToSqlSafely(session, maxSelector, grpAlias);

            var joinNeeded = sourceTable != includeTable;

            var whereCondition = GetWhereCondition(command, includes);

            var groupByFieldSelect = joinNeeded
                                         ? $"(select {groupBy} from {includeTable} as {grpAlias} where {grpAlias}.{includeId} = {connectorField.TypedLocator})"
                                         : $"{groupBy.Replace($"{grpAlias}.", martenDefaultAlias)}";
            var maxFieldSelect = joinNeeded
                                     ? $"(select {max} from {includeTable} as {grpAlias} where {grpAlias}.{includeId} = {connectorField.TypedLocator})"
                                     : $"{max.Replace($"{grpAlias}.", martenDefaultAlias)}";

            var joinId         = joinNeeded ? connectorField.TypedLocator.Replace(martenDefaultAlias, $"{srcLatestAlias}.") : null;
            var groupByAliased = groupBy.Replace($"{grpAlias}.", $"{(joinNeeded ? incAlias : srcLatestAlias)}.");
            var maxAliased     = max.Replace($"{grpAlias}.", $"{(joinNeeded ? incAlias : srcLatestAlias)}.");

            var join = joinNeeded ? $"inner join {includeTable} as {incAlias} on {incAlias}.{includeId} = {joinId}" : "";

            var inQuery = @$"(
select {groupByAliased}, max({maxAliased})
from {sourceTable} as {srcLatestAlias} {join}
where {whereCondition.Replace(martenDefaultAlias, $"{srcLatestAlias}.")}
group by {groupByAliased}
)";

            var condition = $"({groupByFieldSelect}, {maxFieldSelect}) in {inQuery}";
            condition = Regex.Replace(condition, @":p(\d+)", "?");

            return source.Where(x => x.MatchesSql(condition, parameters));
        }

        private static string GetWhereCondition(NpgsqlCommand command, IEnumerable<object> includes)
        {
            var query             = command.CommandText;
            var indexOfFirstWhere = query.IndexOf("where ");
            var endOfFirstQuery   = includes.Any() ? query.IndexOf(");") : query.Length; // when no includes => single query 

            if (indexOfFirstWhere == -1 || indexOfFirstWhere >= endOfFirstQuery)
                return "1 = 1";

            var whereCondition = query.Substring(indexOfFirstWhere + 6, endOfFirstQuery - indexOfFirstWhere - 6);

            return whereCondition;
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
