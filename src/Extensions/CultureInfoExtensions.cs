using System.Globalization;

namespace RecShark.Extensions
{
    public static class CultureInfoExtensions
    {
        public static void Use(this CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture   = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }

        public static void UseDefault(this CultureInfo cultureInfo)
        {
            cultureInfo.Use();
            CultureInfo.DefaultThreadCurrentCulture   = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}
