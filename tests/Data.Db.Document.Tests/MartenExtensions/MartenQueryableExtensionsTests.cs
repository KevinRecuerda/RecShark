﻿using System;
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
            var actual = GetControlsByLogDescription(session, new[] {"FR001", "FR002"}).ToList();
            controls[3].Logs = new[] {new Log("FR001")};
            actual.Should().HaveCount(3);
            actual.Should().BeEquivalentTo(controls[0], controls[2], controls[3]);
        }

        [Fact]
        public async Task WhereArray__Should_not_apply_filter_When_null_or_empty_parameters()
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
            GetControlsByLogDescription(session).Should().BeEquivalentTo(controls);
            GetControlsByLogDescription(session, null).Should().BeEquivalentTo(controls);
        }

        private static IEnumerable<Control> GetControlsByLogDescription(IDocumentSession session, params string[] descriptions)
        {
            return session.Query<Control>()
                          .WhereArray(session, c => c.Logs, a => a.Description, descriptions)
                          .ToList();
        }
    }
}
