using System.Collections.Generic;
using System.Linq;

namespace RecShark.Algorithm
{
    public static class Sorter<T>
    {
        public static Dictionary<int, List<T>> TopologicalDepthSort(IEnumerable<T> items, IEnumerable<(T, T)> edges)
        {
            var groupedEdges = edges.GroupBy(e => e.Item1)
                .Select(g => (g.Key, g.Select(x => x.Item2).ToArray()))
                .ToList();
            return TopologicalDepthSort(items, groupedEdges);
        }

        public static Dictionary<int, List<T>> TopologicalDepthSort(IEnumerable<T> items, IEnumerable<(T, T[])> edges)
        {
            var nodes = items.Select(i => new Node<T>(i)).ToList();

            AddRelationships(nodes, edges);
            TopologicalSort(nodes);

            return GroupByDepth(nodes);
        }

        private static void AddRelationships(IEnumerable<Node<T>> nodes, IEnumerable<(T, T[])> edges)
        {
            var nodeById = nodes.ToDictionary(n => n.Id, n => n);
            foreach (var edge in edges)
            {
                if (!nodeById.TryGetValue(edge.Item1.GetHashCode(), out var node))
                    continue;

                foreach (var dependency in edge.Item2)
                {
                    if (!nodeById.TryGetValue(dependency.GetHashCode(), out var child))
                        continue;

                    node.Children.Add(child);
                    child.Parents.Add(node);
                }
            }
        }

        private static void TopologicalSort(ICollection<Node<T>> nodes)
        {
            var checkedNodes = new HashSet<Node<T>>();

            var nodesToCheck = new Queue<Node<T>>(nodes.Where(n => n.Children.Count == 0));
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
                throw new CyclicDependencyException<T>(cyclicDependencies);
        }

        private static Dictionary<int, List<T>> GroupByDepth(IEnumerable<Node<T>> nodes)
        {
            return nodes.GroupBy(m => m.Depth)
                .ToDictionary(g => g.Key, g => g.Select(n => n.Value).ToList());
        }
    }
}