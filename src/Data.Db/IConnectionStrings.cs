using Microsoft.Extensions.Configuration;

namespace RecShark.Data.Db
{
    public interface IConnectionStrings
    {
        IConnectionString Get(string name);
    }

    public class ConnectionStrings : IConnectionStrings
    {
        private readonly IConfiguration configuration;

        public ConnectionStrings(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public virtual IConnectionString Get(string name)
        {
            return new ConnectionString(name, this.configuration);
        }
    }
}