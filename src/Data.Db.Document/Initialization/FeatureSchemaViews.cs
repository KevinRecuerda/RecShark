using Baseline;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Marten;
using System.Threading.Tasks;
using Weasel.Core;
using Weasel.Core.Migrations;
using DbCommandBuilder = Weasel.Core.DbCommandBuilder;

namespace RecShark.Data.Db.Document.Initialization
{
    public abstract class FeatureSchemaViews : FeatureSchemaBase
    {
        private readonly Lazy<SchemaViews> schemaViews;

        public FeatureSchemaViews(StoreOptions options) : base($"{options.DatabaseSchemaName}._views", options.Advanced.Migrator)
        {
            schemaViews = new Lazy<SchemaViews>(() => new SchemaViews(Identifier, options.DatabaseSchemaName, BuildViews()));
        }

        public abstract string Filename { get; }

        protected override IEnumerable<ISchemaObject> schemaObjects()
        {
            yield return schemaViews.Value;
        }

        private Dictionary<string, string> BuildViews()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Filename);
            var lines    = File.ReadAllText(filepath);

            var matches = Regex.Matches(lines, @"-- (?<name>\w+)[\n\r]+(?<sql>[^;]+);");

            var views = new Dictionary<string, string>();
            foreach (Match match in matches)
            {
                var name = match.Groups["name"].Value;
                var sql  = match.Groups["sql"].Value;
                views[name] = sql;
            }

            return views;
        }
    }

    public class SchemaViews : ISchemaObject
    {
        public SchemaViews(string identifier, string schema, Dictionary<string, string> views)
        {
            Identifier = new DbObjectName(schema, identifier);
            Views      = views;
        }

        public DbObjectName Identifier { get; }

        public Dictionary<string, string> Views { get; }

        public void WriteCreateStatement(Migrator migrator, TextWriter writer)
        {
            var drops   = Views.Keys.Reverse().Select(ToDrop).ToList();
            var creates = Views.Select(x => ToCreate(x.Key, x.Value)).ToList();

            var statements = drops.Union(creates).ToList();
            var sql        = string.Join(Environment.NewLine, statements);
            writer.WriteLine(sql);
        }

        public void WriteDropStatement(Migrator migrator, TextWriter writer)
        {
            var drops = Views.Keys.Reverse().Select(ToDrop).ToList();
            var sql   = string.Join(Environment.NewLine, drops);
            writer.WriteLine(sql);
        }

        public void ConfigureQueryCommand(DbCommandBuilder builder)
        {
            var schema = builder.AddParameter(Identifier.Schema).ParameterName;

            builder.Append(
                $@"
select viewname, definition
from pg_catalog.pg_views
where schemaname = :{schema};
");
        }
        
        public Task<ISchemaObjectDelta> CreateDelta(DbDataReader reader)
        {
            var diff = CheckDifference(reader);

            ISchemaObjectDelta delta = new SchemaObjectDelta(this, diff);
            return Task.FromResult(delta);
        }
        
        public IEnumerable<DbObjectName> AllNames()
        {
            return Views.Keys.Select(v => new DbObjectName(Identifier.Schema, v)).ToList();
        }

        private SchemaPatchDifference CheckDifference(DbDataReader reader)
        {
            var views = new Dictionary<string, string>();
            while (reader.Read())
            {
                var name       = reader.GetString(0);
                var definition = reader.GetString(1);
                views[name] = definition;
            }

            foreach (var item in Views)
            {
                if (!views.TryGetValue(item.Key, out var definition))
                    return SchemaPatchDifference.Create;

                var actual   = definition.CanonicalizeSql();
                var expected = item.Value.CanonicalizeSql().ExtendSelect(actual);
                if (actual != expected)
                    return SchemaPatchDifference.Update;
            }

            return SchemaPatchDifference.None;
        }

        private string ToCreate(string name, string body)
        {
            var sql = $@"CREATE VIEW {Identifier.Schema}.{name} AS 
{body};";
            return sql;
        }

        private string ToDrop(string name)
        {
            var sql = $@"DROP VIEW IF EXISTS {Identifier.Schema}.{name};";
            return sql;
        }
    }

    public static class SqlExtensions
    {
        public static string CanonicalizeSql(this string sql)
        {
            // CanonicizeSql moved to Weasel.Postgresql.Canonicalization (internal class)
            // should generate SQL using Weasel ?
            var replaced = sql.MartenCanonicizeSql()
                              .Replace("\"",                       "")
                              .Replace("(",                        "")
                              .Replace(")",                        "")
                              .Replace(" ->> ",                    "->>")
                              .Replace(" -> ",                     "->")
                              .Replace("::text",                   "")
                              .Replace("::boolean",                "::bool")
                              .Replace("timestamp with time zone", "timestamptz");

            var keywords = new[] {"SELECT ", " AS ", " FROM ", " JOIN ", " ON ", "WITH ", " GROUP BY ", "REGEXP_REPLACE"};
            foreach (var keyword in keywords)
                replaced = Regex.Replace(replaced, keyword, keyword, RegexOptions.IgnoreCase);

            return replaced;
        }

        // TODO - check if can use Weasel to generate Canonical sql
        // => remove Marten* extensions
        public static string MartenCanonicizeSql(this string sql)
        {
            var replaced = sql
                          .Trim()
                          .Replace('\n', ' ')
                          .Replace('\r', ' ')
                          .Replace('\t', ' ')
                          .Replace("!=", "<>")
                          .MartenReplaceMultiSpace(" ")
                          .Replace(" ;",                             ";")
                          .Replace("SECURITY INVOKER",               "")
                          .Replace("  ",                             " ")
                          .Replace("LANGUAGE plpgsql AS $function$", "")
                          .Replace("$$ LANGUAGE plpgsql",            "$function$")
                          .Replace("AS $$ DECLARE",                  "DECLARE")
                          .Replace("character varying",              "varchar")
                          .Replace("Boolean",                        "boolean")
                          .Replace("bool,",                          "boolean,")
                          .Replace("int[]",                          "integer[]")
                          .Replace("numeric",                        "decimal")
                          .TrimEnd(';')
                          .TrimEnd();

            if (replaced.ContainsIgnoreCase("PLV8"))
            {
                replaced = replaced
                   .Replace("LANGUAGE plv8 IMMUTABLE STRICT AS $function$", "AS $$");

                const string languagePlv8ImmutableStrict = "$$ LANGUAGE plv8 IMMUTABLE STRICT";
                const string functionMarker              = "$function$";
                if (replaced.EndsWith(functionMarker))
                {
                    replaced = replaced.Substring(0, replaced.LastIndexOf(functionMarker)) + languagePlv8ImmutableStrict;
                }
            }

            return replaced
                  .Replace("  ", " ")
                  .TrimEnd()
                  .TrimEnd(';');
        }

        public static string MartenReplaceMultiSpace(this string str, string newStr)
        {
            var regex = new Regex("\\s+");
            return regex.Replace(str, newStr);
        }

        public static string ExtendSelect(this string sql, string comparison)
        {
            if (!sql.Contains(".*"))
                return sql;

            var stars     = Regex.Matches(sql, @"(?<id>\w+)\.\*");
            var selectors = comparison.GetSelectors();
            foreach (Match star in stars)
            {
                var id       = star.Groups["id"].Value;
                var selector = selectors.Single(s => s.StartsWith($"{id}.") || s.Contains($" {id}."));
                var extend   = Regex.Match(selector, $@"{id}.\w+(, {id}.\w+)*");
                sql = sql.Replace($"{id}.*", extend.Value);
            }

            return sql;
        }

        public static List<string> GetSelectors(this string sql)
        {
            var splits = sql.Split(new[] {"SELECT "}, StringSplitOptions.None).ToList();
            splits.RemoveAt(0);
            var selectors = splits.Select(
                                       s =>
                                       {
                                           var i = s.IndexOf(" FROM ", StringComparison.Ordinal);
                                           return i == -1 ? s : s.Substring(0, i);
                                       })
                                  .ToList();
            return selectors;
        }
    }
}
