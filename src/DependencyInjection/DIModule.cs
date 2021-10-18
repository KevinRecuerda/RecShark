using Microsoft.Extensions.DependencyInjection;

namespace RecShark.DependencyInjection
{
    public abstract class DIModule
    {
        public string Name => GetType().FullName;

        public virtual DIModule[] Dependencies => new DIModule[] { };

        public abstract void Load(IServiceCollection services);

        public override bool Equals(object obj)
        {
            if (!(obj is DIModule other))
                return false;

            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}