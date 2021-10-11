using Microsoft.Extensions.DependencyInjection;

namespace RecShark.Extensions.DependencyInjection
{
    public abstract class DIModule
    {
        public string Name => this.GetType().FullName;

        public virtual DIModule[] Dependencies => new DIModule[] { };

        public abstract void Load(IServiceCollection services);

        public override bool Equals(object obj)
        {
            if (!(obj is DIModule other))
                return false;

            return this.Name == other.Name;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}