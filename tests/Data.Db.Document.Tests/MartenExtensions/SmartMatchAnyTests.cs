using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Extensions;
using Xunit;

namespace RecShark.Data.Db.Document.Tests.MartenExtensions
{
    public class SmartMatchAnyTests : BaseDocTests
    {
        public SmartMatchAnyTests(DocHooks hooks = null) : base(hooks) { }

        public override void Dispose()
        {
            this.Hooks.Cleaner.DeleteDocumentsFor(typeof(Item));
        }

        [Fact]
        public async Task SmartMatchAny__Should_return_items_according_to_filter()
        {
            // Arrange
            var items = new[]
            {
                new Item {Id = "10", Name = "test 1|FR", Type = ItemType.A},
                new Item {Id = "20", Name = "test 2|FR", Type = ItemType.A},
                new Item {Id = "30", Name = "test 3|ES", Type = ItemType.B},
            };

            var       documentStore = this.Hooks.Provider.GetService<IDocumentStore>();
            using var session       = documentStore.OpenSession();
            session.Store(items);
            await session.SaveChangesAsync();

            // Act & Assert
            await AssertSmartMatchAny(session, null, items[0], items[1], items[2]);

            await AssertSmartMatchAny(session, new[] {"test*"}, items[0], items[1], items[2]);
            await AssertSmartMatchAny(session, new[] {"test"});

            await AssertSmartMatchAny(session, new[] {"test 1*"}, items[0]);
            await AssertSmartMatchAny(session, new[] {"*FR"}, items[0], items[1]);
        }

        private static async Task AssertSmartMatchAny(IQuerySession session, string[] patterns = null, params object[] expected)
        {
            var actual = await session.Query<Item>()
                                      .Where(x => x.Name.SmartMatchAny(patterns))
                                      .ToListAsync();

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
