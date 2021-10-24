using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace RecShark.DependencyInjection
{
    public static class DIModuleLoader
    {
        public static void Load<T>(this IServiceCollection services)
            where T : DIModule, new()
        {
            services.Load(new T());
        }

        public static void Load(this IServiceCollection services, params DIModule[] modules)
        {
            var all = FindAllModules(modules);

            var sortedModulesByDepth = DIModuleSorter.Sort(all);

            foreach (var depthModules in sortedModulesByDepth.OrderBy(x => x.Key))
            {
                foreach (var module in depthModules.Value)
                    module.Load(services);
            }
        }

        public static ICollection<DIModule> FindAllModules(IEnumerable<DIModule> modules)
        {
            var hash = new HashSet<DIModule>();

            var queue = new Queue<DIModule>(modules);
            while (queue.Count > 0)
            {
                var module = queue.Dequeue();
                hash.Add(module);

                foreach (var dependency in module.Dependencies)
                {
                    if (!hash.Contains(dependency))
                        queue.Enqueue(dependency);
                }
            }

            return hash.ToList();
        }
    }
}