using System;
using System.Collections.Generic;
using System.Linq;

namespace RecShark.DependencyInjection.Sorter
{
    public class CyclicDependencyException : Exception
    {
        public CyclicDependencyException(ICollection<DIModule> modules)
            : base(
                $@"Cyclic dependency found between following module ids
{string.Join(Environment.NewLine, modules.Select(m => m.Name))}")
        {
            Modules = modules;
        }

        public ICollection<DIModule> Modules { get; }
    }
}