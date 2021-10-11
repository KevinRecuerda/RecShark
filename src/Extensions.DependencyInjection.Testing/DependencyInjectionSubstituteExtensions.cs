using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RecShark.Extensions.DependencyInjection.Testing
{
    public static class DependencyInjectionSubstituteExtensions
    {
        public static IServiceCollection Substitute(this IServiceCollection services, params ServiceDescriptor[] serviceDescriptors)
        {
            foreach (var serviceDescriptor in serviceDescriptors)
            {
                services.Remove(serviceDescriptor);

                var substitute = Substitutor.Substitute(serviceDescriptor.ServiceType);
                services.AddSingleton(serviceDescriptor.ServiceType, substitute);
            }

            return services;
        }

        public static IServiceCollection Substitute(this IServiceCollection services, Func<ServiceDescriptor, bool> condition)
        {
            var serviceDescriptors = services.Where(condition).ToArray();
            return services.Substitute(serviceDescriptors);
        }

        public static bool InheritedFrom(this ServiceDescriptor service, params string[] names)
        {
            var type = service.ImplementationType;

            while (type != null)
            {
                if (names.Any(name => type.Name.Contains(name)))
                    return true;

                type = type.BaseType;
            }

            return false;
        }
    }

    public class Substitutor
    {
        private static readonly MethodInfo SubstituteMethodInfo =
            typeof(Substitutor).GetMethod(nameof(Substitute), BindingFlags.Static|BindingFlags.NonPublic);

        public static object Substitute(Type type)
        {
            var substitute = SubstituteMethodInfo.MakeGenericMethod(type).Invoke(new Substitutor(), null);
            return substitute;
        }

        private static T Substitute<T>()
            where T : class
        {
            return NSubstitute.Substitute.For<T>();
        }
    }
}
