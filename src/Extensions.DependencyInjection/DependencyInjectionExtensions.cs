using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace RecShark.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static void Reset(this IServiceCollection services, ServiceDescriptor serviceDescriptor)
        {
            services.Remove(serviceDescriptor.ServiceType);
            services.Add(serviceDescriptor);
        }

        public static void Remove<T>(this IServiceCollection services)
        {
            services.Remove(typeof(T));
        }

        public static void Remove(this IServiceCollection services, Type serviceType)
        {
            var serviceDescriptors = services.Where(x => x.ServiceType == serviceType).ToList();
            foreach (var serviceDescriptor in serviceDescriptors)
                services.Remove(serviceDescriptor);
        }

        public static void RemoveImpl<T>(this IServiceCollection services)
        {
            services.RemoveImpl(typeof(T));
        }

        public static void RemoveImpl(this IServiceCollection services, Type implementationType)
        {
            var serviceDescriptors = services.Where(x => GetImplementationType(x) == implementationType).ToList();
            foreach (var serviceDescriptor in serviceDescriptors)
                services.Remove(serviceDescriptor);
        }

        public static void AddSingleton<TService1, TService2, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TService1, TService2
            where TService1 : class
            where TService2 : class
        {
            services.AddSingleton<TImplementation>();
            services.AddSingleton<TService1, TImplementation>(x => x.GetRequiredService<TImplementation>());
            services.AddSingleton<TService2, TImplementation>(x => x.GetRequiredService<TImplementation>());
        }

        public static IServiceCollection AddSingletonIfNotNull<TImplementation>(
            this IServiceCollection services,
            TImplementation implementationInstance)
            where TImplementation : class
        {
            if (implementationInstance != null)
                services.AddSingleton(implementationInstance);
            return services;
        }

        public static Type GetImplementationType(ServiceDescriptor serviceDescriptor)
        {
            return serviceDescriptor.ImplementationType
                   ?? serviceDescriptor.ImplementationInstance?.GetType()
                   ?? serviceDescriptor.ImplementationFactory?.GetType().GenericTypeArguments[1];
        }
    }
}