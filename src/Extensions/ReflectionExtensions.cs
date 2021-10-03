using System;
using System.Linq;
using System.Reflection;

namespace RecShark.Extensions
{
    public static class ReflectionExtensions
    {
        public static object InvokeMethodSafely(this object obj, string methodName, params object[] parameters)
        {
            return obj.GetType().GetMethod(methodName)?.Invoke(obj, parameters);
        }

        public static bool IsNullableType(this Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public static string[] GetConst(this Type type)
        {
            return type.GetFields(BindingFlags.Static | BindingFlags.Public)
                       .Select(f => f.GetRawConstantValue().ToString())
                       .ToArray();
        }
    }
}