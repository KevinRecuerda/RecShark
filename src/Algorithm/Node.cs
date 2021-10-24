using System.Collections.Generic;

namespace RecShark.Algorithm
{
    public class Node<T>
    {
        public Node(T value)
        {
            Id = value.GetHashCode();
            Value = value;

            Parents = new List<Node<T>>();
            Children = new List<Node<T>>();
            Depth = 0;
        }

        public int Id { get; }
        public T Value { get; }

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