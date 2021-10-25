using System.Globalization;
 using RecShark.Testing.FluentAssertions;

 namespace RecShark.Testing
{
    public class Tests
    {
        static Tests()
        {
            FluentAssertionsExtensions.UsePrecision();
            OverrideCultureInfo();
        }

        public static void OverrideCultureInfo(CultureInfo cultureInfo = null)
        {
            cultureInfo                               ??= CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture                =   cultureInfo;
            CultureInfo.CurrentUICulture              =   cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture   =   cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture =   cultureInfo;
        }
    }
}
