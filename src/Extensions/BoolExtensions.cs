namespace RecShark.Extensions
{
    public static class BoolExtensions
    {
        public static string ToString(this bool? x, string trueValue = "", string falseValue = "", string nullValue = "")
        {
            return !x.HasValue ? nullValue :
                   x.Value    ? trueValue : falseValue;
        }
    }
}