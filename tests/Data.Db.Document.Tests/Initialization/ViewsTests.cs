using Marten;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Data.Db.Document.Initialization;
using RecShark.Data.Db.Document.Testing;
using Xunit;

namespace RecShark.Data.Db.Document.Tests.Initialization
{
    using Weasel.Core;

    public class ViewsTests : BaseDocTests
    {
        private readonly DocumentStore docStore;

        public ViewsTests(DocHooks hooks = null) : base(hooks)
        {
            docStore = (DocumentStore)Hooks.Provider.GetService<IDocumentStore>();
        }

        [Fact]
        public void Should_contains_views() => FeatureSchemaViewsTests<Views>.Should_contains_views(docStore, 1);

        // [Fact]
        // public void Should_not_reapply_twice() => FeatureSchemaViewsTests<Views>.Should_not_reapply_twice(docStore);
    }

    public class Views : FeatureSchemaViews
    {
        public Views(StoreOptions options) : base(options) { }

        public override string Filename => @"Initialization/views.sql";
    }
}
