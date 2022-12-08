namespace RecShark.Data.Db.Document.Tests.MartenExtensions;

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Extensions;
using Xunit;

public class IsBetweenTests : BaseDocTests
{
    public IsBetweenTests(DocHooks hooks = null) : base(hooks) { }

    public override void Dispose()
    {
        Hooks.Cleaner.CompletelyRemove(typeof(Control));
    }

    [Fact]
    public async Task IsBetween__Should_return_items_according_to_filter()
    {
        // Arrange
        var controls = new[]
        {
            new Control {Date = new DateTime(2000, 12, 27)},
            new Control {Date = new DateTime(2000, 12, 28)},
            new Control {Date = new DateTime(2000, 12, 29)},
            new Control {Date = new DateTime(2000, 12, 30)},
            new Control {Date = new DateTime(2000, 12, 31)},
        };

        var             documentStore = Hooks.Provider.GetService<IDocumentStore>();
        await using var session       = documentStore.OpenSession();
        session.Store(controls);
        await session.SaveChangesAsync();
        
        // Act & Assert
        var actual = await session.Query<Control>().Where(c => c.Date.IsBetween(controls[1].Date, controls[3].Date)).ToListAsync();
        actual.Should().BeEquivalentTo(controls[1], controls[2], controls[3]);
        
        actual = await session.Query<Control>().Where(c => c.Date.IsBetween(controls[3].Date, null)).ToListAsync();
        actual.Should().BeEquivalentTo(controls[3], controls[4]);
        
        actual = await session.Query<Control>().Where(c => c.Date.IsBetween(null, controls[1].Date)).ToListAsync();
        actual.Should().BeEquivalentTo(controls[0], controls[1]);        
    }
}
