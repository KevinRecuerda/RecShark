using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RecShark.Extensions
{
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
            return type.GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(f => f.GetRawConstantValue().ToString())
                .ToArray();
        }

        public static void RunStaticConstructor(this Type type)
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
    }
}