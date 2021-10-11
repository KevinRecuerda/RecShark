using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Marten;
using Marten.Schema;
using Marten.Storage;
using Marten.Util;

namespace RecShark.Data.Db.Document.Initialization
{
    public abstract class FeatureSchemaViews : FeatureSchemaBase
    {
        private readonly Lazy<SchemaViews> schemaViews;

        public FeatureSchemaViews(StoreOptions options) : base($"{options.DatabaseSchemaName}._views", options)
        {
            this.schemaViews = new Lazy<SchemaViews>(() => new SchemaViews(this.Identifier, this.Options.DatabaseSchemaName, this.BuildViews()));
        }

        public abstract string Filename { get; }

        protected override IEnumerable<ISchemaObject> schemaObjects()
        {
            yield return this.schemaViews.Value;
        }

        private Dictionary<string, string> BuildViews()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.Filename);
            var lines    = File.ReadAllText(filepath);

            var matches = Regex.Matches(
                lines,
                @"-- (?<name>\w+)
(?<sql>[^;]+);");

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
            this.Identifier = new DbObjectName(identifier);
            this.Schema     = schema;
            this.Views      = views;
        }

        public DbObjectName Identifier { get; }

        public string Schema { get; }

        public Dictionary<string, string> Views { get; }

        public IEnumerable<DbObjectName> AllNames()
        {
            return this.Views.Keys.Select(v => new DbObjectName(this.Schema, v)).ToList();
        }

        public void ConfigureQueryCommand(CommandBuilder builder)
        {
            var schema = builder.AddParameter(this.Schema).ParameterName;

            builder.Append(
                $@"
select viewname, definition
from pg_catalog.pg_views
where schemaname = :{schema};
");
        }

        public SchemaPatchDifference CreatePatch(DbDataReader reader, SchemaPatch patch, AutoCreate autoCreate)
        {
            var diff = this.CheckDifference(reader);
            if (diff != SchemaPatchDifference.None)
            {
                this.Write(patch.Rules, patch.UpWriter);
                this.WriteDropStatement(patch.Rules, patch.DownWriter);
            }

            return diff;
        }

        public void Write(DdlRules rules, StringWriter writer)
        {
            var drops   = this.Views.Keys.Reverse().Select(this.ToDrop).ToList();
            var creates = this.Views.Select(x => this.ToCreate(x.Key, x.Value)).ToList();

            var statements = drops.Union(creates).ToList();
            var sql        = string.Join(Environment.NewLine, statements);
            writer.WriteLine(sql);
        }

        public void WriteDropStatement(DdlRules rules, StringWriter writer)
        {
            var drops = this.Views.Keys.Reverse().Select(this.ToDrop).ToList();
            var sql   = string.Join(Environment.NewLine, drops);
            writer.WriteLine(sql);
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

            foreach (var item in this.Views)
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
            var sql = $@"CREATE VIEW {this.Schema}.{name} AS 
{body};";
            return sql;
        }

        private string ToDrop(string name)
        {
            var sql = $@"DROP VIEW IF EXISTS {this.Schema}.{name};";
            return sql;
        }
    }

    public static class SqlExtensions
    {
        public static string CanonicalizeSql(this string sql)
        {
            var replaced = sql.CanonicizeSql()
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
            var splits = sql.Split(new[] {"SELECT "}, StringSplitOptions.None).RemoveAt(0);
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