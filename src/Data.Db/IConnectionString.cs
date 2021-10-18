using Microsoft.Extensions.Configuration;

namespace RecShark.Data.Db
{
    public interface IConnectionString
    {
        string Name  { get; }
        string Value { get; }
    }

    public class ConnectionString : IConnectionString
    {
        public ConnectionString(string name, IConfiguration configuration)
        {
            Name  = name;
            Value = configuration.GetConnectionString(Name);
        }

        public string Name  { get; set; }
        public string Value { get; set; }
    }
}