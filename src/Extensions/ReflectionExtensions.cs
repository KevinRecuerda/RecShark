using System.Reflection;

namespace RecShark.Extensions
{
    public static class ReflectionExtensions
    {
        public static object InvokeMethodSafely(this object obj, string methodName, params object[] parameters)
        {
            return obj.GetType().GetMethod(methodName)?.Invoke(obj, parameters);
        }

        public static T GetFieldValue<T>(this object obj, string name, BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            var fi = obj.GetType().GetField(name, bindingAttr);
            return (T) fi?.GetValue(obj);
        }
    }
}
