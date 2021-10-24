using System;
using System.Collections.Generic;
using System.Linq;
using RecShark.Extensions;

namespace RecShark.Algorithm
{
    public class CyclicDependencyException<T> : Exception
    {
        public CyclicDependencyException(ICollection<Node<T>> nodes)
            : base(BuildMessage(nodes))
        {
            Nodes = nodes;
        }

        public ICollection<Node<T>> Nodes { get; }

        private static string BuildMessage(ICollection<Node<T>> nodes)
        {
            return $@"Cyclic dependency found between following {typeof(T).Name}
{nodes.Select(n => n.Value).ToLines()}";
        }
    }
}