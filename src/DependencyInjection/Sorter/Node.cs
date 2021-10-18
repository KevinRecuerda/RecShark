using System.Collections.Generic;

namespace RecShark.DependencyInjection.Sorter
{
    public class Node<T>
    {
        public Node(string id, T item)
        {
            Id = id;
            Item = item;

            Parents = new List<Node<T>>();
            Children = new List<Node<T>>();
            Depth = 0;
        }

        public string Id { get; }
        public T Item { get; }

        public List<Node<T>> Parents { get; }
        public List<Node<T>> Children { get; }

        public int Depth { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is Node<T> other))
                return false;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}