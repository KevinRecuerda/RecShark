namespace RecShark.Data.Db.Document.Initialization;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Marten;
using Weasel.Core;
using Weasel.Core.Migrations;
using DbCommandBuilder = Weasel.Core.DbCommandBuilder;

public class FeatureSchemaIndex : FeatureSchemaBase
{
    private readonly Lazy<SchemaIndexes> schemaIndex;

    public FeatureSchemaIndex(StoreOptions options, string indexName, string table, string definition)
        : base($"{options.DatabaseSchemaName}._index_{indexName}", options.Advanced.Migrator)
    {
        schemaIndex = new Lazy<SchemaIndexes>(() => new SchemaIndexes(indexName, options.DatabaseSchemaName, table, definition));
    }

    protected override IEnumerable<ISchemaObject> schemaObjects()
    {
        yield return schemaIndex.Value;
    }
}

public class SchemaIndexes : ISchemaObject
{
    private readonly string table;
    private readonly string definition;
    private readonly string createStatement;

    public SchemaIndexes(string indexName, string schema, string table, string definition)
    {
        this.Identifier      = new DbObjectName(schema, indexName);
        this.table           = table;
        this.definition      = definition;
        this.createStatement = $@"CREATE INDEX {indexName} ON {schema}.{this.table} USING {this.definition}";
    }

    public DbObjectName Identifier { get; }

    public void WriteCreateStatement(Migrator migrator, TextWriter writer)
    {
        //writer.WriteLine(this.createStatement);
    }

    public void WriteDropStatement(Migrator migrator, TextWriter writer)
    {
        // Drop statement is automatically done ?
        //writer.WriteLine($"DROP INDEX {this.indexName}");
    }

    public void ConfigureQueryCommand(DbCommandBuilder builder)
    {
        var schema = builder.AddParameter(this.Identifier.Schema).ParameterName;

        builder.Append(
            $@"
select indexname, indexdef 
from pg_catalog.pg_indexes
WHERE
    schemaname = :{schema}");
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

        if (!indexes.TryGetValue(this.Identifier.Schema, out var definition))
            return SchemaPatchDifference.Create;

        var actual   = definition.CanonicalizeSql();
        var expected = this.createStatement.CanonicalizeSql();
        if (actual != expected)
            return SchemaPatchDifference.Update;

        return SchemaPatchDifference.None;
    }
}
