using System.Collections.Generic;
using System.Linq;
using RecShark.Algorithm;

namespace RecShark.DependencyInjection
{
    public static class DIModuleSorter
    {
        public static Dictionary<int, List<DIModule>> Sort(ICollection<DIModule> modules)
        {
            var edges = modules.Select(m => (m, m.Dependencies)).ToList();
            return Sorter<DIModule>.TopologicalDepthSort(modules, edges);
        }
    }
}