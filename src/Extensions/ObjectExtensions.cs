using FastDeepCloner;
using System.Linq;

namespace RecShark.Extensions
{
    public static class ObjectExtensions
    {
        public static T Clone<T>(this T item)
        {
            return item.GetType().IsInternalType()
            ? item
            : (T)((object)item).Clone(null);
        }

        public static void CloneTo(this object item, object target)
        {
            DeepCloner.CloneTo(item, target);
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
