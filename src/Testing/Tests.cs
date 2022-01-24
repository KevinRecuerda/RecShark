using System.Globalization;
using RecShark.Testing.FluentAssertions;

namespace RecShark.Testing
{
    using RecShark.Extensions;

    public class Tests
    {
        static Tests()
        {
            FluentAssertionsExtensions.UsePrecision();
            CultureInfo.InvariantCulture.UseDefault();
        }
    }
}
