using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using RecShark.Extensions;

namespace RecShark.Data.Db.Document.Tests.MartenExtensions
{
    public class IsInTests : BaseDocTests
    {
        public IsInTests(DocHooks hooks = null) : base(hooks) { }

        public override void Dispose()
        {
            Hooks.Cleaner.CompletelyRemove(typeof(Item));
        }

        [Fact]
        public async Task IsIn__Should_return_items_according_to_filter()
        {
            // Arrange
            var items = new[]
            {
                new Item {Id = "10", Name = "test 1", Type = ItemType.A},
                new Item {Id = "20", Name = "test 2", Type = ItemType.A},
                new Item {Id = "30", Name = "test 3", Type = ItemType.B},
            };

            var       documentStore = Hooks.Provider.GetService<IDocumentStore>();
            using var session       = documentStore.OpenSession();
            session.Store(items);
            await session.SaveChangesAsync();

            // Act & Assert
            await AssertIsIn(session, null,             null,               null, items[0], items[1], items[2]);
            await AssertIsIn(session, null,             new ItemType[] { }, null, items[0], items[1], items[2]);
            await AssertIsIn(session, new string[] { }, null,               null, items[0], items[1], items[2]);
            
            await AssertIsIn(session, new[] {"10", "30"}, null, null, items[0], items[2]);
            await AssertIsIn(session, new[] {"40"},       null);

            await AssertIsIn(session, null, new[] {ItemType.B},             null, items[2]);
            await AssertIsIn(session, null, new[] {ItemType.A, ItemType.B}, null, items[0], items[1], items[2]);

            await AssertIsIn(session, new[] {"10"}, new[] {ItemType.A}, null, items[0]);
        }

        [Fact]
        public async Task IsIn__Should_manage_null_values()
        {
            // Arrange
            var items = new[]
            {
                new Item {Id = "10", Name = "test 1", Type = ItemType.A},
                new Item {Id = "20", Name = null, Type     = ItemType.A},
                new Item {Id = "30", Name = "", Type       = ItemType.B},
            };

            var       documentStore = Hooks.Provider.GetService<IDocumentStore>();
            using var session       = documentStore.OpenSession();
            session.Store(items);
            await session.SaveChangesAsync();

            // Act & Assert
            await AssertIsIn(session, null, null, new[] {"test 1"}, items[0]);
            await AssertIsIn(session, null, null, new[] {""},       items[2]);
            await AssertIsIn(session, null, null, new[] {(string) null});
        }

        private static async Task AssertIsIn(
            IQuerySession   session,
            string[]        ids   = null,
            ItemType[]      types = null,
            string[]        names = null,
            params object[] expected)
        {
            var actual = await session.Query<Item>()
                                      .Where(x => x.Id.IsIn(ids))
                                      .Where(x => x.Name.IsIn(names))
                                      .Where(x => x.Type.IsIn(types))
                                      .ToListAsync();

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
