namespace RecShark.Data.Db.Document.Tests.MartenExtensions;

using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Data.Db.Document.MartenExtensions;
using Xunit;

public class SearchSimilarExtensionsTests : BaseDocTests
{
    public SearchSimilarExtensionsTests(DocHooks hooks = null) : base(hooks) { }

    public override void Dispose()
    {
        Hooks.Cleaner.CompletelyRemove(typeof(Item));
    }

    [Fact(Skip="postgresql extension pg_trgm not installed")]
    public async Task SearchSimilarAsync__Should_return_items_according_to_search()
    {
        // Arrange
        var items = new[]
        {
            new Item {Id = "10", Name = "hello world", Type = ItemType.A},
            new Item {Id = "20", Name = "bla bla", Type     = ItemType.A},
            new Item {Id = "30", Name = "test 3", Type      = ItemType.B},
        };

        var       documentStore = Hooks.Provider.GetService<IDocumentStore>();
        using var session       = documentStore.OpenSession();
        session.Store(items);
        await session.SaveChangesAsync();

        // Act
        var actual = await session.SearchSimilarAsync<Item>(i => i.Name, "test", 2);

        // Assert
        actual.Should().BeEquivalentTo(items[2], items[0]);
    }
}
