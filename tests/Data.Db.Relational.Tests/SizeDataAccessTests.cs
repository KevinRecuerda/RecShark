using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace RecShark.Data.Db.Relational.Tests
{
    public class SizeDataAccessTests : BaseRelTests
    {
        [Fact]
        public async Task Should_return_public_schema_size()
        {
            // Arrange
            var sizeDataAccess = this.Hooks.Provider.GetService<ISizeDataAccess>();

            // Act
            var actual = await sizeDataAccess.GetSchemaSize();

            // Assert
            actual.Should().NotBeEmpty();
            actual.Should().Contain(x => x.Id == "public");
        }
    }
}