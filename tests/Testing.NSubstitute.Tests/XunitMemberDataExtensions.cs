namespace RecShark.Testing.NSubstitute.Tests
{
    public class Labeled<T>
    {
        public Labeled(string label, T data)
        {
            Label = label;
            Data = data;
        }

        public string Label { get; }
        public T Data { get; }

        public object[] ToInput()
        {
            return new object[] {this};
        }

        public override string ToString()
        {
            return Label;
        }
    }

    public static class LabeledExtensions
    {
        public static object[] LabeledData<T>(this string label, T data)
        {
            return new Labeled<T>(label, data).ToInput();
        }
    }
}