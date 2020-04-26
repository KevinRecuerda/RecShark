using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Exceptions;
using RecShark.Extensions.Testing.FluentAssertions;
using RecShark.Extensions.Testing.FluentAssertions.Tests;
using Xunit;

namespace RecShark.Extensions.Testing.NSubstitute.Tests
{
    public class ArgExtensionsTests
    {
        private readonly IServiceForTests service = Substitute.For<IServiceForTests>();

        private static ObjectForTests obj1 = new ObjectForTests(1, 3.14, DateTime.Today);
        private static ObjectForTests obj2 = new ObjectForTests(2, 3.15, DateTime.Today);

        [Fact]
        public void AsArg__Should_match_object_using_equivalence()
        {
            // Arrange
            service.Do(obj1);
            var expected = obj1.Clone();

            // Act
            Action action = () => service.Received(1).Do(expected);
            Action actionArg = () => service.Received(1).Do(expected.AsArg());

            // Assert
            action.Should().Throw<ReceivedCallsException>();
            actionArg.Should().NotThrow();
        }

        [Fact]
        public void AsArg__Should_match_object_with_options()
        {
            // Arrange
            service.Do(obj1);
            var expected = obj1.Clone();
            expected.Value = -100;

            // Act
            Action action = () => service.Received(1).Do(expected);
            Action actionArg = () => service.Received(1).Do(expected.AsArg());
            Action actionArgOption = () => service.Received(1).Do(expected.AsArg(options => options.Excluding(x => x.Value)));

            // Assert
            action.Should().Throw<ReceivedCallsException>();
            actionArg.Should().Throw<ReceivedCallsException>();
            actionArgOption.Should().NotThrow();
        }

        [Fact]
        public void AsArg__Should_throw_exception_when_object_does_not_match()
        {
            // Arrange
            service.Do(obj1);
            var expected = obj1.Clone();
            expected.Value = -100;

            // Act
            Action actionArg = () => service.Received(1).Do(expected.AsArg());

            // Assert
            actionArg.Should().Throw<ReceivedCallsException>();
        }

        [Fact]
        public void AsArg__Should_match_array_using_equivalence()
        {
            // Arrange
            var items = new[] {obj1, obj2};
            service.DoMultiple(items);
            var expected = new[] {obj1, obj2};

            // Act
            Action action = () => service.Received(1).DoMultiple(expected);
            Action actionArg = () => service.Received(1).DoMultiple(expected.AsArg());

            // Assert
            action.Should().Throw<ReceivedCallsException>();
            actionArg.Should().NotThrow();
        }

        [Fact]
        public void AsArg__Should_match_array_with_options()
        {
            // Arrange
            var items = new[] {obj1, obj2};
            service.DoMultiple(items);
            service.Do(obj1);

            var obj1Bis = obj1.Clone();
            obj1Bis.Value = -100;
            var expected = new[] {obj1Bis, obj2};

            // Act
            Action action = () => service.Received(1).DoMultiple(expected);
            Action actionArg = () => service.Received(1).DoMultiple(expected.AsArg());
            Action actionArgOption = () =>
                service.Received(1)
                    .DoMultiple(expected.AsArg(options => options.ExcludingSubCollectionMember("Value")));

            // Assert
            action.Should().Throw<ReceivedCallsException>();
            actionArg.Should().Throw<ReceivedCallsException>();
            actionArgOption.Should().NotThrow();
        }

        [Fact]
        public void AsArg__Should_throw_exception_when_array_does_not_match()
        {
            // Arrange
            var items = new[] {obj1, obj2};
            service.DoMultiple(items);
            var expected = new[] {obj1};

            // Act
            Action action = () => service.Received(1).DoMultiple(expected.AsArg());

            // Assert
            action.Should().Throw<ReceivedCallsException>();
        }

        public static IEnumerable<object[]> EnumerableData =>
            CollectionData.Concat(
                new List<object[]>
                {
                    "enumerable".LabeledData<object>(Enumerable.Range(0, 2).Select(i => i == 0 ? obj1 : obj2))
                });

        public static IEnumerable<object[]> CollectionData =>
            new List<object[]>
            {
                "array".LabeledData<object>(new[] {obj1, obj2}),
                "list".LabeledData<object>(new List<ObjectForTests> {obj1, obj2})
            };

        [Theory]
        [MemberData(nameof(EnumerableData))]
        public void AsArg__Should_match_enumerable(Labeled<object> items)
        {
            // Arrange
            service.DoMultiple((IEnumerable<ObjectForTests>) items.Data);
            var expected = new[] {obj1, obj2};

            // Act
            Action action = () => service.Received(1).DoMultiple(expected);
            Action actionArg = () => service.Received(1).DoMultiple(expected.AsArgEnumerable());

            // Assert
            action.Should().Throw<ReceivedCallsException>();
            actionArg.Should().NotThrow();
        }

        [Theory]
        [MemberData(nameof(CollectionData))]
        public void AsArg__Should_match_collection(Labeled<object> items)
        {
            // Arrange
            service.DoMultipleCollection((ICollection<ObjectForTests>) items.Data);
            var expected = new[] {obj1, obj2};

            // Act
            Action action = () => service.Received(1).DoMultipleCollection(expected);
            Action actionArg = () => service.Received(1).DoMultipleCollection(expected.AsArgCollection());

            // Assert
            action.Should().Throw<ReceivedCallsException>();
            actionArg.Should().NotThrow();
        }
    }
}