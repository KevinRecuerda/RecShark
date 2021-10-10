using System.Linq;

namespace RecShark.Extensions
{
    public static class ObjectExtensions
    {
        public static T Clone<T>(this T original)
        {
            return DeepCopy.DeepCopier.Copy(original);
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