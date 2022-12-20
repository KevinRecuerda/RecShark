namespace RecShark.Data.Db.Document.Tests.MartenExtensions;

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Extensions;
using Xunit;

public class ContainsAnyTests : BaseDocTests
{
    public ContainsAnyTests(DocHooks hooks = null) : base(hooks) { }

    public override void Dispose()
    {
        Hooks.Cleaner.CompletelyRemove(typeof(Control));
    }

    [Fact]
    public async Task ContainsAny__Should_return_items_according_to_filter()
    {
        // Arrange
        var controls = new[]
        {
            new Control() {Data = null},
            new Control() {Data = new[] {"A"}},
            new Control() {Data = new[] {"B"}},
            new Control() {Data = new[] {"A", "B", "C"}},
            new Control() {Data = new[] {"C"}}
        };

        using var session = Hooks.Provider.GetService<IDocumentStore>().OpenSession();
        session.Store(controls);
        await session.SaveChangesAsync();

        // Act
        var actual = await session.Query<Control>()
                                  .Where(c => c.Data.ContainsAny("A", "B"))
                                  .ToListAsync();

        // Assert
        actual.Should().BeEquivalentTo(controls[1], controls[2], controls[3]);
    }
}
