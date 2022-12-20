using System.Linq;
using FluentAssertions;
using Marten;
using RecShark.Data.Db.Document.Initialization;
using Weasel.Core;

namespace RecShark.Data.Db.Document.Testing
{
    public static class FeatureSchemaViewsTests<T>
        where T : FeatureSchemaViews
    {
        public static void Should_contains_views(DocumentStore documentStore, int viewsCount)
        {
            // Arrange
            var views = CreateViews(documentStore.Options);

            // Act
            var schemaViews = (SchemaViews) views.Objects.First();

            // Assert
            schemaViews.Views.Count.Should().Be(viewsCount);
        }

        public static void Should_not_reapply_twice(DocumentStore documentStore)
        {
            // Arrange
            var views = CreateViews(documentStore.Options);
            documentStore.Schema.ApplyAllConfiguredChangesToDatabaseAsync().Wait();

            // Act
            var patch = documentStore.Schema.CreateMigrationAsync().Result;

            // Assert
            var actual = patch.Deltas.Single(d => d.SchemaObject.Identifier.Name == views.Identifier);
            actual.Difference.Should().Be(SchemaPatchDifference.None);
        }

        private static T CreateViews(StoreOptions options)
        {
            var views = (T) options.Storage.FindFeature(typeof(T));

            var schemaViews = (SchemaViews) views.Objects.First();

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
