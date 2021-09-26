using System.Collections.Generic;
using RecShark.Testing.FluentAssertions.Tests;

namespace RecShark.Testing.NSubstitute.Tests
{
    public interface IServiceForTests
    {
        void Do(ObjectForTests item);

        void DoMultiple(IEnumerable<ObjectForTests> items);

        void DoMultipleCollection(ICollection<ObjectForTests> items);
    }
}