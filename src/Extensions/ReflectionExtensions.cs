namespace RecShark.Extensions
{
    public static class ReflectionExtensions
    {
        public static object InvokeMethodSafely(this object obj, string methodName, params object[] parameters)
        {
            return obj.GetType().GetMethod(methodName)?.Invoke(obj, parameters);
        }
    }
}