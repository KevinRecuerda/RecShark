using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RecShark.Extensions.DependencyInjection.Sorter;
using Xunit;

namespace RecShark.Extensions.DependencyInjection.Tests.Sorter
{
    public class DIModuleSorterTests
    {
        [Fact]
        public void Sort__Should_correctly_sort_modules_by_depth()
        {
            // Arrange
            var modules = new DIModule[] {new DIModuleA(), new DIModuleB(), new DIModuleC()};

            // Act
            var actual = DIModuleSorter.Sort(modules);

            // Assert
            actual.Count.Should().Be(3);
            actual[0].Should().BeEquivalentTo(new DIModuleC());
            actual[1].Should().BeEquivalentTo(new DIModuleB());
            actual[2].Should().BeEquivalentTo(new DIModuleA());
        }

        [Fact]
        public void Sort__Should_throw_cyclic_dependency_exception_When_module_depends_on_itself()
        {
            // Arrange
            var modules = new DIModule[] {new DIModuleCyclic()};

            // Act
            Func<Dictionary<int, List<DIModule>>> action = () => DIModuleSorter.Sort(modules);

            // Assert
            var exception = action.Should().Throw<CyclicDependencyException>();
            exception.And.Modules.Should().BeEquivalentTo(modules.Cast<object>().ToArray());
        }

        [Fact]
        public void Sort__Should_throw_cyclic_dependency_exception_Having_direct_cyclic_dependency()
        {
            // Arrange
            var modules = new DIModule[] {new DIModuleCyclic1(), new DIModuleCyclic2()};

            // Act
            Func<Dictionary<int, List<DIModule>>> action = () => DIModuleSorter.Sort(modules);

            // Assert
            var exception = action.Should().Throw<CyclicDependencyException>();
            exception.And.Modules.Should().BeEquivalentTo(modules.Cast<object>().ToArray());
        }

        [Fact]
        public void Sort__Should_throw_cyclic_dependency_exception_Having_indirect_cyclic_dependency()
        {
            // Arrange
            var modules = new DIModule[] {new DIModuleCyclicA(), new DIModuleCyclicB(), new DIModuleCyclicC()};

            // Act
            Func<Dictionary<int, List<DIModule>>> action = () => DIModuleSorter.Sort(modules);

            // Assert
            var exception = action.Should().Throw<CyclicDependencyException>();
            exception.And.Modules.Should().BeEquivalentTo(modules.Cast<object>().ToArray());
        }
    }

    public class DIModuleCyclicForTests<T> : DIModuleForTests
        where T : DIModule, new()
    {
        public override DIModule[] Dependencies => new DIModule[] {new T()};
    }

    public class DIModuleCyclic : DIModuleCyclicForTests<DIModuleCyclic> { }

    public class DIModuleCyclic1 : DIModuleCyclicForTests<DIModuleCyclic2> { }

    public class DIModuleCyclic2 : DIModuleCyclicForTests<DIModuleCyclic1> { }

    public class DIModuleCyclicA : DIModuleCyclicForTests<DIModuleCyclicB> { }

    public class DIModuleCyclicB : DIModuleCyclicForTests<DIModuleCyclicC> { }

    public class DIModuleCyclicC : DIModuleCyclicForTests<DIModuleCyclicA> { }
}