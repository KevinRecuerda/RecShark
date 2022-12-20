namespace RecShark.Data.Db.Document.Initialization;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Weasel.Core;
using Weasel.Core.Migrations;
using DbCommandBuilder = Weasel.Core.DbCommandBuilder;

public class FeatureSchemaIndexes<T> : FeatureSchemaBase
{
    private readonly SchemaIndexes schemaIndexes;

    public FeatureSchemaIndexes(StoreOptions options, string table, params (string name, string definition)[] indexes)
        : base($"{options.DatabaseSchemaName}._index_{table}", options.Advanced.Migrator)
    {
        foreach (var index in indexes)
            options.Schema.For<T>().IgnoreIndex(index.name);
        
        this.schemaIndexes  = new SchemaIndexes(options.DatabaseSchemaName, table, indexes);
    }

    protected override IEnumerable<ISchemaObject> schemaObjects()
    {
        yield return this.schemaIndexes;
    }
}

public class SchemaIndexes : ISchemaObject
{
    private readonly string                     table;
    public           Dictionary<string, string> Indexes { get; }
    private          List<string>               createStatements;
    private          List<string>               dropStatements;

    public SchemaIndexes(string schema, string table, params (string indexName, string definition)[] indexes)
    {
        this.table      = table;
        this.Identifier = new DbObjectName(schema, $"{table}_indexes");

        this.Indexes = indexes.ToDictionary(i => i.indexName, i => i.definition);
    }

    public DbObjectName Identifier { get; }

    public void WriteCreateStatement(Migrator migrator, TextWriter writer)
    {
        foreach (var createStatement in this.createStatements)
        {
            writer.WriteLine(createStatement);
        }
    }

    public void WriteDropStatement(Migrator migrator, TextWriter writer)
    {
        foreach (var dropStatement in this.dropStatements)
        {
            writer.WriteLine(dropStatement);
        }
    }

    public void ConfigureQueryCommand(DbCommandBuilder builder)
    {
        var schema = builder.AddParameter(this.Identifier.Schema).ParameterName;

        builder.Append(
            $@"
select indexname, indexdef 
from pg_catalog.pg_indexes
WHERE schemaname = :{schema};");
    }

    public Task<ISchemaObjectDelta> CreateDelta(DbDataReader reader)
    {
        var diff = CheckDifference(reader);

        ISchemaObjectDelta delta = new SchemaObjectDelta(this, diff);
        return Task.FromResult(delta);
    }

    public IEnumerable<DbObjectName> AllNames()
    {
        return new[] {this.Identifier};
    }

    private SchemaPatchDifference CheckDifference(DbDataReader reader)
    {
        var indexes = new Dictionary<string, string>();
        while (reader.Read())
        {
            var dbName       = reader.GetString(0);
            var dbDefinition = reader.GetString(1);
            indexes[dbName] = dbDefinition;
        }

        this.createStatements = new List<string>();
        this.dropStatements   = new List<string>();
        foreach (var index in this.Indexes)
        {
            var create = this.ToCreate(index.Key, index.Value);
            if (!indexes.TryGetValue(index.Key, out var definition))
            {
                this.createStatements.Add(create);
            }
            else
            {
                var actual   = definition.CanonicalizeSql();
                var expected = create.CanonicalizeSql();
                if (actual != expected)
                {
                    this.dropStatements.Add(ToDrop(index.Key));
                    this.createStatements.Add(create);
                }
            }
        }

        if (this.createStatements.IsEmpty() && this.dropStatements.IsEmpty())
            return SchemaPatchDifference.None;

        return SchemaPatchDifference.Update;
    }

    private string ToCreate(string name, string definition)
    {
        var sql = $@"CREATE INDEX {name} ON {this.Identifier.Schema}.{this.table} USING {definition};";
        return sql;
    }

    private string ToDrop(string name)
    {
        var sql = $"DROP INDEX {this.Identifier.Schema}.{name};";
        return sql;
    }
}
