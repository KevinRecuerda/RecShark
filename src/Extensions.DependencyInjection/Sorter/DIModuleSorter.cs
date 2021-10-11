using System.Collections.Generic;
using System.Linq;

namespace RecShark.Extensions.DependencyInjection.Sorter
{
    public static class DIModuleSorter
    {
        public static Dictionary<int, List<DIModule>> Sort(IEnumerable<DIModule> modules)
        {
            var nodes = CreateNodes(modules);

            AddRelationships(nodes);
            TopologicalSort(nodes);

            return GroupByDepth(nodes);
        }

        private static ICollection<Node<DIModule>> CreateNodes(IEnumerable<DIModule> modules)
        {
            var nodes = modules.Select(module => new Node<DIModule>(module.Name, module)).ToList();
            return nodes;
        }

        private static void AddRelationships(ICollection<Node<DIModule>> nodes)
        {
            var nodeById = nodes.ToDictionary(n => n.Id, n => n);
            foreach (var node in nodes)
            {
                foreach (var dependency in node.Item.Dependencies)
                {
                    if (!nodeById.TryGetValue(dependency.Name, out var child))
                        continue;

                    node.Children.Add(child);
                    child.Parents.Add(node);
                }
            }
        }

        private static void TopologicalSort(ICollection<Node<DIModule>> nodes)
        {
            var checkedNodes = new HashSet<Node<DIModule>>();

            var nodesToCheck = new Queue<Node<DIModule>>(nodes.Where(n => n.Children.Count == 0));
            while (nodesToCheck.Any())
            {
                var node = nodesToCheck.Dequeue();
                checkedNodes.Add(node);

                foreach (var parent in node.Parents)
                {
                    parent.Depth = node.Depth + 1;

                    parent.Children.Remove(node);
                    if (parent.Children.Count == 0)
                        nodesToCheck.Enqueue(parent);
                }

                node.Parents.Clear();
            }

            var cyclicDependencies = nodes.Except(checkedNodes).ToList();
            if (cyclicDependencies.Any())
                throw new CyclicDependencyException(cyclicDependencies.Select(x => x.Item).ToList());
        }

        private static Dictionary<int, List<DIModule>> GroupByDepth(IEnumerable<Node<DIModule>> nodes)
        {
            return nodes.GroupBy(m => m.Depth)
                .ToDictionary(g => g.Key, g => g.Select(n => n.Item).ToList());
        }
    }
}