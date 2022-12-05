using System.Linq;
using FluentAssertions;
using Marten;
using Marten.Schema;
using Marten.Storage;
using RecShark.Data.Db.Document.Initialization;

namespace RecShark.Data.Db.Document.Testing
{
    using Weasel.Core;

    public static class FeatureSchemaViewsTests<T>
        where T : FeatureSchemaViews
    {
        public static void Should_contains_views(DocumentStore documentStore, int viewsCount)
        {
            // Arrange
            var views = CreateViews(documentStore.Options);

            // Act
            var schemaViews = (SchemaViews)views.Objects.First();

            // Assert
            schemaViews.Views.Count.Should().Be(viewsCount);
        }

        // TODO: rewrite this test
        // public static void Should_not_reapply_twice(DocumentStore documentStore)
        // {
        //     // Arrange
        //     var views = CreateViews(documentStore.Options);
        //
        //     documentStore.Schema.ApplyAllConfiguredChangesToDatabaseAsync().Wait();
        //
        //     // Act
        //     // await documentStore.Storage.Database.WriteMigrationFileAsync("1.initial.sql");
        //     // var patch = documentStore.Schema.CreateMigrationAsync().Result;
        //     var patch = new SchemaPatch(documentStore.Options.DdlRules);
        //     using (var session = documentStore.OpenSession())
        //     {
        //         patch.Apply(session.Connection, documentStore.Options.AutoCreateSchemaObjects, views.Objects);
        //     }
        //
        //     // Assert
        //     var actual = patch.Migrations.Single(m => m.SchemaObject.Identifier.Name == views.Identifier);
        //     actual.Difference.Should().Be(SchemaPatchDifference.None);
        // }

        private static T CreateViews(StoreOptions options)
        {
            var views = (T)options.Storage.FindFeature(typeof(T));

            var schemaViews = (SchemaViews)views.Objects.First();

            // Adapt schema
            var schema = schemaViews.Identifier.Schema.Replace("_tests", "");
            foreach (var view in schemaViews.Views.ToList())
            {
                schemaViews.Views[view.Key] = view.Value.Replace(schema + ".", schemaViews.Identifier.Schema + ".");
            }

            return views;
        }
    }
}
