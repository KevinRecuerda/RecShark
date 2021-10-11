namespace RecShark.Extensions.DependencyInjection.Tests
{
    public class Sample : ISample, IOther
    {
        public string Hello() => "Hello";
    }

    public interface ISample
    {
        string Hello();
    }

    public interface IOther { }
}