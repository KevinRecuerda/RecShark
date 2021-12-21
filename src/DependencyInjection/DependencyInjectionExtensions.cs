using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RecShark.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        private static IConfiguration config;

        public static T AddConfig<T>(this IServiceCollection services, string name)
            where T : class
        {
            // cache config for performance
            // it should have no impact, as config should be loaded at first
            config ??= services.BuildServiceProvider().GetService<IConfiguration>();

            var section = config.GetSection(name).Get<T>();
            services.TryAddSingleton(section, true);
            return section;
        }

        public static IServiceCollection TryAddSingleton<T>(this IServiceCollection services, T instance, bool ignoreNull)
            where T : class
        {
            if (ignoreNull && instance == null)
                return services;

            services.TryAddSingleton(instance);
            return services;
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

        public static Type GetImplementationType(ServiceDescriptor serviceDescriptor)
        {
            return serviceDescriptor.ImplementationType
                ?? serviceDescriptor.ImplementationInstance?.GetType()
                ?? serviceDescriptor.ImplementationFactory?.GetType().GenericTypeArguments[1];
        }
    }
}
