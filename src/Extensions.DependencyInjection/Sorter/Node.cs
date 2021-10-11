using System.Collections.Generic;

namespace RecShark.Extensions.DependencyInjection.Sorter
{
    public class Node<T>
    {
        public Node(string id, T item)
        {
            this.Id = id;
            this.Item = item;

            this.Parents = new List<Node<T>>();
            this.Children = new List<Node<T>>();
            this.Depth = 0;
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

            return this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}