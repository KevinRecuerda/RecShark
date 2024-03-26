using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RecShark.Extensions
{
    using System.Text;

    public static class TypeExtensions
    {
        public static bool IsNullableType(this Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public static Type AsDictionary(this Type type)
        {
            var interfaces = type.GetInterfaces().ToList();
            if (type.IsInterface)
                interfaces.Insert(0, type);

            return interfaces.FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        public static Type[] GetDictionaryInnerTypes(this Type type)
        {
            var dictType = type.AsDictionary();
            return dictType?.GetGenericArguments() ?? Type.EmptyTypes;
        }

        public static string[] GetConstValues(this Type type)
        {
            return type.GetFields(BindingFlags.Static|BindingFlags.Public)
                       .Select(f => f.GetRawConstantValue().ToString())
                       .ToArray();
        }

        public static void RunStaticConstructor(this Type type)
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }

        public static string AsFirstName(this Type type)
        {
            var name = type.Name;
            if (type.IsGenericType) 
            {
                #if (NET6_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER)
                    name = name[..name.IndexOf('`')];
                #else
                    name = name.Substring(0, name.IndexOf('`'));
                #endif
            }

            return name;
        }

        public static string AsFriendlyName(this Type type)
        {
            var name = type.AsFirstName();
            if (!type.IsGenericType)
                return name;

            var genericArgumentIds = type.GetGenericArguments()
                                         .Select(AsFriendlyName)
                                         .ToArray();

            return new StringBuilder(name)
                  .Append($"[{genericArgumentIds.ToString(",")}]")
                  .ToString();
        }
    }
}
