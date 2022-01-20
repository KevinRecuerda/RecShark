using System.Linq;

namespace RecShark.Extensions
{
    public static class ObjectExtensions
    {
        public static T Clone<T>(this T original)
            where T : class
        {
            return FastDeepCloner.DeepCloner.Clone<T>(original, null);
        }

        public static void CloneTo(this object original, object target)
        {
            FastDeepCloner.DeepCloner.CloneTo(original, target);
        }

        public static void AdaptSmart<T>(T item)
        {
            var props = typeof(T).GetProperties().Where(p => p.PropertyType == typeof(object)).ToList();
            foreach (var prop in props)
            {
                var value = prop.GetValue(item, null);
                var smartValue = ConvertExtensions.ConvertSmart(value);
                prop.SetValue(item, smartValue);
            }
        }
    }
}
