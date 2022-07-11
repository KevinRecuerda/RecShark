using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Data.Db.Document.MartenExtensions;
using Xunit;

namespace RecShark.Data.Db.Document.Tests.MartenExtensions
{
    public class MartenQueryableExtensionsTests : BaseDocTests
    {
        public MartenQueryableExtensionsTests(DocHooks hooks = null) : base(hooks) { }

        public override void Dispose()
        {
            Hooks.Cleaner.DeleteDocumentsFor(typeof(Control));
            Hooks.Cleaner.DeleteDocumentsFor(typeof(Item));

            base.Dispose();
        }

        [Fact]
        public async Task Latest__Should_return_latest_controls()
        {
            // Arrange
            var items = new[]
            {
                new Item {Id = "1", Name = "test 1"},
                new Item {Id = "2", Name = "test 2"}
            };
            var controls = new[]
            {
                new Control() {Date = new DateTime(2000, 12, 30), ItemId = "1", Result = 10},
                new Control() {Date = new DateTime(2000, 12, 31), ItemId = "1", Result = 20},
                new Control() {Date = new DateTime(2000, 12, 30), ItemId = "2", Result = 30}
            };

            var       documentStore = Hooks.Provider.GetService<IDocumentStore>();
            using var session       = documentStore.OpenSession();
            session.Store(items);
            session.Store(controls);
            await session.SaveChangesAsync();

            // Act
            var actualItems = new List<Item>();
            var actual = await session.Query<Control>()
                                      .Include(c => c.ItemId, actualItems)
                                      .Latest(session, c => c.Date, c => c.ItemId)
                                      .ToListAsync();

            // Assert
            actual.Should().HaveCount(2);
            actual.Should().BeEquivalentTo(controls[1], controls[2]);
            actualItems.Should().HaveCount(2);
            actualItems.Should().BeEquivalentTo(items.Cast<object>());
        }

        [Fact]
        public async Task Latest__Should_return_latest_controls_according_to_filter()
        {
            // Arrange
            var items = new[]
            {
                new Item {Id = "1", Name = "test 1"},
                new Item {Id = "2", Name = "test 2"}
            };
            var controls = new[]
            {
                new Control() {Date = new DateTime(2000, 12, 30), ItemId = "1", Result = 10},
                new Control() {Date = new DateTime(2000, 12, 31), ItemId = "1", Result = 20},
                new Control() {Date = new DateTime(2000, 12, 30), ItemId = "2", Result = 30}
            };

            var       documentStore = Hooks.Provider.GetService<IDocumentStore>();
            using var session       = documentStore.OpenSession();
            session.Store(items);
            session.Store(controls);
            await session.SaveChangesAsync();

            // Act
            var actualItems = new List<Item>();
            var actual = await session.Query<Control>()
                                      .Include(c => c.ItemId, actualItems)
                                      .Where(c => c.Result == 10)
                                      .Latest(session, c => c.Date, c => c.ItemId)
                                      .ToListAsync();

            // Assert
            actual.Should().HaveCount(1);
            actual.Should().BeEquivalentTo(controls[0]);
            actualItems.Should().HaveCount(1);
            actualItems.Should().BeEquivalentTo(items[0]);
        }

        [Fact]
        public async Task Latest__Should_return_latest_controls_When_grouping_on_multiple_columns()
        {
            // Arrange
            var items = new[]
            {
                new Item {Id = "1", Name = "test 1"},
                new Item {Id = "2", Name = "test 2"}
            };
            var controls = new[]
            {
                new Control() {Date = new DateTime(2000, 12, 30), ItemId = "1", Result = 10},
                new Control() {Date = new DateTime(2000, 12, 30), ItemId = "1", Result = 20},
                new Control() {Date = new DateTime(2000, 12, 31), ItemId = "1", Result = 25},
                new Control() {Date = new DateTime(2000, 12, 30), ItemId = "2", Result = 30}
            };

            var       documentStore = Hooks.Provider.GetService<IDocumentStore>();
            using var session       = documentStore.OpenSession();
            session.Store(items);
            session.Store(controls);
            await session.SaveChangesAsync();

            // Act
            var actualItems = new List<Item>();
            var actual = await session.Query<Control>()
                                      .Include(c => c.ItemId, actualItems)
                                      .Latest(session, c => c.Result, c => c.ItemId, c => c.Date)
                                      .ToListAsync();

            // Assert
            actual.Should().HaveCount(3);
            actual.Should().BeEquivalentTo(controls[1], controls[2], controls[3]);
            actualItems.Should().HaveCount(2);
            actualItems.Should().BeEquivalentTo(items.Cast<object>());
        }

        [Fact]
        public async Task Latest__Should_return_latest_controls_When_grouping_on_multiple_columns_and_null_value()
        {
            // Arrange
            var controls = new[]
            {
                new Control {Date = new DateTime(2000, 12, 30), Result = 10},
                new Control {Date = new DateTime(2000, 12, 30), Result = 20},
                new Control {Date = new DateTime(2000, 12, 31), Result = 25},
                new Control {Date = new DateTime(2000, 12, 30), Result = 30}
            };

            var       documentStore = Hooks.Provider.GetService<IDocumentStore>();
            using var session       = documentStore.OpenSession();
            session.Store(controls);
            await session.SaveChangesAsync();

            // Act
            var actual = await session.Query<Control>()
                                      .Latest(session, c => c.Result, c => c.Date, c => c.ItemId)
                                      .ToListAsync();

            // Assert
            actual.Should().HaveCount(2);
            actual.Should().BeEquivalentTo(controls[2], controls[3]);
        }

        [Fact]
        public async Task Latest__Should_return_latest_controls_When_grouping_on_multiple_columns_according_to_filter()
        {
            // Arrange
            var items = new[]
            {
                new Item {Id = "1", Name = "test 1"},
                new Item {Id = "2", Name = "test 2"}
            };
            var controls = new[]
            {
                new Control(new DateTime(2000, 12, 30), "1", 10),
                new Control(new DateTime(2000, 12, 30), "1", 20),
                new Control(new DateTime(2000, 12, 31), "1", 25),
                new Control(new DateTime(2000, 12, 30), "2", 30)
            };

            var       documentStore = Hooks.Provider.GetService<IDocumentStore>();
            using var session       = documentStore.OpenSession();
            session.Store(items);
            session.Store(controls);
            await session.SaveChangesAsync();

            // Act
            var actualItems = new List<Item>();
            var actual = await session.Query<Control>()
                                      .Include(c => c.ItemId, actualItems)
                                      .Where(c => c.ItemId == "1")
                                      .Latest(session, c => c.Result, c => c.ItemId, c => c.Date)
                                      .ToListAsync();

            // Assert
            actual.Should().HaveCount(2);
            actual.Should().BeEquivalentTo(controls[1], controls[2]);
            actualItems.Should().HaveCount(1);
            actualItems.Should().BeEquivalentTo(items[0]);
        }

        [Fact]
        public async Task LatestInclude__Should_return_latest_controls()
        {
            // Arrange
            var items = new[]
            {
                new Item {Id = "1", Name = "test 1", Type = ItemType.A},
                new Item {Id = "2", Name = "test 2", Type = ItemType.A},
                new Item {Id = "3", Name = "test 3", Type = ItemType.B},
                new Item {Id = "4", Name = "test 4", Type = ItemType.B}
            };
            var controls = new[]
            {
                new Control() {ItemId = "1", Result = 10},
                new Control() {ItemId = "2", Result = 20},
                new Control() {ItemId = "3", Result = 30}
            };

            var       documentStore = Hooks.Provider.GetService<IDocumentStore>();
            using var session       = documentStore.OpenSession();
            session.Store(items);
            session.Store(controls);
            await session.SaveChangesAsync();

            // Act
            var actualItems = new List<Item>();
            var actual = await session.Query<Control>()
                                      .Include(c => c.ItemId, actualItems)
                                      .Latest<Control, Item>(session, i => i.Id, i => i.Type)
                                      .ToListAsync();

            // Assert
            actual.Should().HaveCount(2);
            actual.Should().BeEquivalentTo(controls[1], controls[2]);
            actualItems.Should().HaveCount(2);
            actualItems.Should().BeEquivalentTo(items[1], items[2]);
        }

        [Fact]
        public async Task LatestInclude__Should_return_latest_controls_according_to_filter()
        {
            // Arrange
            var items = new[]
            {
                new Item {Id = "1", Name = "test 1", Type = ItemType.A},
                new Item {Id = "2", Name = "test 2", Type = ItemType.A},
                new Item {Id = "3", Name = "test 3", Type = ItemType.B},
                new Item {Id = "4", Name = "test 4", Type = ItemType.B}
            };
            var controls = new[]
            {
                new Control() {ItemId = "1", Result = 10},
                new Control() {ItemId = "2", Result = 20},
                new Control() {ItemId = "3", Result = 30}
            };

            var       documentStore = Hooks.Provider.GetService<IDocumentStore>();
            using var session       = documentStore.OpenSession();
            session.Store(items);
            session.Store(controls);
            await session.SaveChangesAsync();

            // Act
            var actualItems = new List<Item>();
            var actual = await session.Query<Control>()
                                      .Include(c => c.ItemId, actualItems)
                                      .Where(c => c.Result == 10)
                                      .Latest<Control, Item>(session, i => i.Id, i => i.Type)
                                      .ToListAsync();

            // Assert
            actual.Should().HaveCount(1);
            actual.Should().BeEquivalentTo(controls[0]);
            actualItems.Should().HaveCount(1);
            actualItems.Should().BeEquivalentTo(items[0]);
        }

        [Fact]
        public async Task Where__Should_filter_on_include_table()
        {
            // Arrange
            var items = new[]
            {
                new Item {Id = "1", Name = "test 1", Type = ItemType.A},
                new Item {Id = "2", Name = "test 2", Type = ItemType.B}
            };
            var controls = new[]
            {
                new Control(new DateTime(2000, 12, 29), "1", 10),
                new Control(new DateTime(2000, 12, 30), "1", 20), // last
                new Control(new DateTime(2000, 12, 31), "2", 30), // last
            };

            using var session = Hooks.Provider.GetService<IDocumentStore>().OpenSession();
            session.Store(items);
            session.Store(controls);

            items[0].LastControlId = controls[1].Id;
            items[1].LastControlId = controls[2].Id;
            session.Store(items);
            await session.SaveChangesAsync();

            // Act
            var actualControls = new Dictionary<Guid, Control>();
            var actual = session.Query<Item>()
                                .Include(i => i.LastControlId, actualControls)
                                .Where<Item, Control>(c => c.Result == 20, session)
                                .ToList();

            // Assert
            actual.Should().HaveCount(1);
            actual.Should().ContainEquivalentOf(items[0]);

            actualControls.Should().HaveCount(1);
            actualControls.Values.Should().ContainEquivalentOf(controls[1]);
        }

        [Fact]
        public async Task Where__Should_filter_on_multiple_include_table()
        {
            // Arrange
            var items = new[]
            {
                new Item {Id = "1", Name = "test 1", Type = ItemType.A},
                new Item {Id = "2", Name = "test 2", Type = ItemType.B}
            };
            var controls = new[]
            {
                new Control(new DateTime(2000, 12, 29), "1", 10),
                new Control(new DateTime(2000, 12, 30), "1", 20), // last
                new Control(new DateTime(2000, 12, 31), "2", 30), // last
            };

            using var session = Hooks.Provider.GetService<IDocumentStore>().OpenSession();
            session.Store(items);
            session.Store(controls);

            items[0].FirstControlId = controls[0].Id;
            items[0].LastControlId = controls[1].Id;
            items[1].FirstControlId = controls[2].Id;
            items[1].LastControlId = controls[2].Id;
            session.Store(items);
            await session.SaveChangesAsync();

            // Act
            var actualFirstControls = new Dictionary<Guid, Control>();
            var actualLastControls = new Dictionary<Guid, Control>();
            var actual = session.Query<Item>()
                                .Include(i => i.FirstControlId, actualFirstControls)
                                .Include(i => i.LastControlId,  actualLastControls)
                                .Where<Item, Control>(c => c.Result == 20, session, 1)
                                .ToList();

            // Assert
            actual.Should().HaveCount(1);
            actual.Should().ContainEquivalentOf(items[0]);

            actualFirstControls.Should().HaveCount(1);
            actualFirstControls.Values.Should().ContainEquivalentOf(controls[0]);

            actualLastControls.Should().HaveCount(1);
            actualLastControls.Values.Should().ContainEquivalentOf(controls[1]);
        }

        [Fact]
        public async Task WhereArray__Should_return_controls_with_only_filtered_logs()
        {
            // Arrange
            var controls = new[]
            {
                new Control(new DateTime(2000, 12, 30), null, 10, new Log("FR002")),
                new Control(new DateTime(2000, 12, 30), null, 20),
                new Control(new DateTime(2000, 12, 31), null, 25, new Log("FR001")),
                new Control(new DateTime(2000, 12, 30), null, 30, new Log("FR001"), new Log("FR004"))
            };

            using var session = Hooks.Provider.GetService<IDocumentStore>().OpenSession();
            session.Store(controls);
            await session.SaveChangesAsync();

            // Act
            var actual = session.Query<Control>()
                                .WhereArray(session, c => c.Logs, a => a.Description, new[] {"FR001", "FR002"})
                                .ToList();

            // Assert
            controls[3].Logs = new[] {new Log("FR001")};
            actual.Should().HaveCount(3);
            actual.Should().BeEquivalentTo(controls[0], controls[2], controls[3]);
        }

        [Fact]
        public async Task WhereArray__Should_not_apply_filter_When_empty_parameters()
        {
            // Arrange
            object[] controls =
            {
                new Control(new DateTime(2000, 12, 30), null, 10, new Log("FR002")),
                new Control(new DateTime(2000, 12, 30), null, 20),
                new Control(new DateTime(2000, 12, 31), null, 25, new Log("FR001")),
                new Control(new DateTime(2000, 12, 30), null, 30, new Log("FR001"), new Log("FR004"))
            };

            using var session = Hooks.Provider.GetService<IDocumentStore>().OpenSession();
            session.Store(controls);
            await session.SaveChangesAsync();

            // Act
            var actual = session.Query<Control>()
                                .WhereArray(session, c => c.Logs, a => a.Description, Array.Empty<string>())
                                .ToList();

            // Assert
            actual.Should().BeEquivalentTo(controls);
        }

        [Fact]
        public async Task WhereArray__Should_return_controls_with_matching_patterns()
        {
            // Arrange
            var controls = new[]
            {
                new Control(new DateTime(2000, 12, 30), null, 10, new Log("FR002")),
                new Control(new DateTime(2000, 12, 30), null, 20),
                new Control(new DateTime(2000, 12, 31), null, 25, new Log("FR001")),
                new Control(new DateTime(2000, 12, 30), null, 30, new Log("FR00001"), new Log("FR004")),
                new Control(new DateTime(2000, 12, 30), null, 10, new Log("US003"))
            };

            using var session = Hooks.Provider.GetService<IDocumentStore>().OpenSession();
            session.Store(controls);
            await session.SaveChangesAsync();

            // Act
            var actual = session.Query<Control>()
                                .WhereLikeArray(session, c => c.Logs, a => a.Description, new[] {"FR*1", "*3"})
                                .ToList();

            // Assert
            controls[3].Logs = new[] {new Log("FR00001")};
            actual.Should().HaveCount(3);
            actual.Should().BeEquivalentTo(controls[2], controls[3], controls[4]);
        }
    }
}
