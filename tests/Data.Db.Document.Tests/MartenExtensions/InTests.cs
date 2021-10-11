using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Extensions;
using Xunit;

namespace RecShark.Data.Db.Document.Tests.MartenExtensions
{
    public class InTests : BaseDocTests
    {
        public InTests(DocHooks hooks = null) : base(hooks) { }

        public override void Dispose()
        {
            this.Hooks.Cleaner.DeleteDocumentsFor(typeof(Item));
        }

        [Fact]
        public async Task In__Should_return_items_according_to_filter()
        {
            // Arrange
            var items = new[]
            {
                new Item {Id = "10", Name = "test 1", Type = ItemType.A},
                new Item {Id = "20", Name = "test 2", Type = ItemType.A},
                new Item {Id = "30", Name = "test 3", Type = ItemType.B},
            };

            var       documentStore = this.Hooks.Provider.GetService<IDocumentStore>();
            using var session       = documentStore.OpenSession();
            session.Store(items);
            await session.SaveChangesAsync();

            // Act & Assert
            await AssertIn(session, null,             null,               items[0], items[1], items[2]);
            await AssertIn(session, null,             new ItemType[] { }, items[0], items[1], items[2]);
            await AssertIn(session, new string[] { }, null,               items[0], items[1], items[2]);
            
            await AssertIn(session, new[] {"10", "30"}, null, items[0], items[2]);
            await AssertIn(session, new[] {"40"});

            await AssertIn(session, null, new[] {ItemType.B},             items[2]);
            await AssertIn(session, null, new[] {ItemType.A, ItemType.B}, items[0], items[1], items[2]);

            await AssertIn(session, new[] {"10"}, new[] {ItemType.A}, items[0]);
        }

        private static async Task AssertIn(IQuerySession session, string[] ids = null, ItemType[] types = null, params object[] expected)
        {
            var actual = await session.Query<Item>()
                                      .Where(x => x.Id.In(ids))
                                      .Where(x => x.Type.In(types))
                                      .ToListAsync();

            actual.Should().BeEquivalentTo(expected);
        }
    }
}