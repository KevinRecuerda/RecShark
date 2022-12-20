using Baseline.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Baseline;
using Marten;
using Marten.Linq.MatchesSql;
using Marten.Internal.Storage;
using Marten.Linq.Fields;
using Marten.Linq.Parsing;
using Npgsql;
using System.Collections;
using System.Reflection;
using RecShark.Extensions;
using Weasel.Postgresql;
using CommandExtensions = Weasel.Postgresql.CommandExtensions;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    public static class MartenQueryableExtensions
    {
        private const string MartenDefaultIdCol      = "id";
        private const string MartenDefaultTableAlias = "d";

        public static IReadOnlyList<TOut> SelectFields<TIn, TOut>(
            this IQueryable<TIn> queryable,
            IDocumentSession     session,
            bool                 leftOuter,
            (string, string)[]   fields,
            params object[]      parameters)
        {
            var command    = queryable.ToCommand();
            var jsonFields = string.Join(",", fields.Select(f => $"'{f.Item1}', {f.Item2}"));

            var includes = GetIncludes(queryable);

            var joins = "";
            foreach (var include in includes)
                joins += BuildJoin(include, null, leftOuter).join;

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

            var arrayCol  = GetMemberInfos(arraySelector).Single().Name;
            var filterCol = GetMemberInfos(filterSelector).Single().Name;

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
            const string sourceTableAlias = "src";

            var (_, _, condition, parameters) = GetEntityNames(session, predicate);
            var (sourceTable, sourceId, _, _) = GetEntityNames<TSource>(session);
            var includes = GetIncludes(source);

            var include = includes.Where(IsIncludeOfType<TInclude>).ElementAt(includeIndex);
            var (join, includeTableAliasPrefix) = BuildJoin(include, sourceTableAlias);

            condition = RenameAlias(condition, MartenDefaultTableAlias, includeTableAliasPrefix);
            condition = Regex.Replace(condition, @":p(\d+)", "?");

            var matchSql = @$"{MartenDefaultTableAlias}.{sourceId} in (
select src.{sourceId} 
from {sourceTable} as {sourceTableAlias} {join} 
where {condition}
)";

            var queryable = source.Where(x => x.MatchesSql(matchSql, parameters));
            return queryable;
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
            const string grpAlias       = "grp";
            const string srcLatestAlias = "srclast";
            const string incAlias       = "inclast";

            var command    = source.ToCommand();
            var parameters = command.Parameters.Select(p => p.Value).ToArray();

            var (sourceTable, _, _, _)  = GetEntityNames<TSource>(session);
            var (includeTable, _, _, _) = GetEntityNames<TInclude>(session);

            var groupBy = groupBySelectors.Select(g => SelectorToSqlSafely(session, g, grpAlias)).Join(",");
            var max     = SelectorToSqlSafely(session, maxSelector, grpAlias);

            string groupByFieldSelect, maxFieldSelect;
            string groupByAliased,     maxAliased, join = "";
            if (sourceTable != includeTable)
            {
                var include = GetIncludes(source).FirstOrDefault(IsIncludeOfType<TInclude>);

                // connectorField.TypedLocator contains d. prefix
                var connectorField = GetPropValue<IField>(include, "ConnectingField");

                groupByFieldSelect =
                    $"(select {groupBy} from {includeTable} as {grpAlias} where {grpAlias}.{MartenDefaultIdCol} = {connectorField.TypedLocator})";
                maxFieldSelect =
                    $"(select {max} from {includeTable} as {grpAlias} where {grpAlias}.{MartenDefaultIdCol} = {connectorField.TypedLocator})";

                groupByAliased = RenameAlias(groupBy, grpAlias, incAlias);
                maxAliased     = RenameAlias(max,     grpAlias, incAlias);

                var joinId = RenameAlias(connectorField.TypedLocator, MartenDefaultTableAlias, srcLatestAlias);
                join = $"inner join {includeTable} as {incAlias} on {incAlias}.{MartenDefaultIdCol} = {joinId}";
            }
            else
            {
                groupByFieldSelect = RenameAlias(groupBy, grpAlias, MartenDefaultTableAlias);
                maxFieldSelect     = RenameAlias(max,     grpAlias, MartenDefaultTableAlias);
                groupByAliased     = RenameAlias(groupBy, grpAlias, srcLatestAlias);
                maxAliased         = RenameAlias(max,     grpAlias, srcLatestAlias);
            }

            var whereCondition = GetWhereCondition(command, GetIncludes(source));

            // TODO! need to replace all aliases !
            whereCondition = RenameAlias(whereCondition, MartenDefaultTableAlias, srcLatestAlias);

            var inQuery = @$"(
select {groupByAliased}, max({maxAliased})
from {sourceTable} as {srcLatestAlias} {join}
where {whereCondition}
group by {groupByAliased}
)";

            var condition = $"({groupByFieldSelect}, {maxFieldSelect}) in {inQuery}";
            condition = Regex.Replace(condition, @":p(\d+)", "?");

            var queryable = source.Where(x => x.MatchesSql(condition, parameters));
            return queryable;
        }

        private static string RenameAlias(string expression, string alias, string newAlias)
        {
            return expression.Replace($"{alias}.", $"{newAlias}.");
        }

        private static (string join, string includeTableAlias) BuildJoin(object include, string sourceTableAlias = null, bool leftOuter = false)
        {
            var connectingField = GetPropValue<IField>(include, "ConnectingField");

            var locator           = connectingField.TypedLocator;
            var includeTable      = GetStorage(include).TableName;
            var includeTableAlias = $"{connectingField.Members.Last().Name}_"; // avoid table alias ending by d

            if (sourceTableAlias != null)
                locator = RenameAlias(locator, MartenDefaultTableAlias, sourceTableAlias);

            var joinType = leftOuter ? "LEFT OUTER JOIN" : "INNER JOIN";
            var join     = $" {joinType} {includeTable.QualifiedName} as {includeTableAlias} on {locator} = {includeTableAlias}.{MartenDefaultIdCol}";
            return (join, includeTableAlias);
        }

        private static (string tableName, string idName, string whereClause, object[] parameters) GetEntityNames<T>(
            IQuerySession             session,
            Expression<Func<T, bool>> predicate = null)
        {
            // generated query template:
            // select d.id, d.data from schema.table as d where condition
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

        private static string GetWhereCondition(NpgsqlCommand command, IEnumerable<object> includes)
        {
            // generated queries templates
            //  - when includes
            // drop temp table temp0; create temp table temp0 as (select id, ... from table where condition);select ...
            // - without includes
            // select id,... from table where condition
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

            return field.FieldType.IsNullableType() ? $"COALESCE({sqlSelector},'')" : sqlSelector;
        }

        private static IField SelectorToField<T, TResult>(IDocumentSession session, Expression<Func<T, TResult>> selector)
        {
            var members     = GetMemberInfos(selector);
            var docProvider = session.Database.Providers.StorageFor<T>();
            var field       = docProvider.Lightweight.Fields.FieldFor(members);
            return field;
        }

        private static IList<MemberInfo> GetMemberInfos<T, TResult>(Expression<Func<T, TResult>> selector)
        {
            var findMembers = new FindMembers();
            findMembers.Visit(selector);
            return findMembers.Members;
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

        private static bool IsIncludeOfType<TInclude>(object include)
        {
            return GetPropValue<Type>(include, "DocumentType") == typeof(TInclude);
        }

        private static IEnumerable<object> GetIncludes(IQueryable source)
        {
            var provider = GetPropValue<object>(source, "MartenProvider");
            var includes = GetPrivatePropValue<ICollection>(provider, "AllIncludes");
            return includes?.Cast<object>();
        }

        private static IDocumentStorage GetStorage(object includePlan)
        {
            return GetPrivateField<IDocumentStorage>(includePlan, "_storage");
        }

        private static T GetPropValue<T>(object includePlan, string propName)
        {
            return (T) includePlan?.GetType().GetProperty(propName)?.GetValue(includePlan);
        }

        private static T GetPrivatePropValue<T>(object o, string propName)
        {
            return (T) o.GetType()
                        .GetProperty(propName, BindingFlags.NonPublic|BindingFlags.Instance)
                       ?.GetValue(o);
        }

        private static T GetPrivateField<T>(object o, string fieldName)
        {
            return (T) o?.GetType()
                         .GetField(fieldName, BindingFlags.NonPublic|BindingFlags.Instance)
                        ?.GetValue(o);
        }
    }
}
