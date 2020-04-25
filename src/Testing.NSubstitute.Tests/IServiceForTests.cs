using System.Collections.Generic;
using RecShark.Extensions.Testing.FluentAssertions.Tests;

namespace RecShark.Extensions.Testing.NSubstitute.Tests
{
    public interface IServiceForTests
    {
        void Do(ObjectForTests item);

        void DoMultiple(IEnumerable<ObjectForTests> items);

        void DoMultipleCollection(ICollection<ObjectForTests> items);
    }
}