using System;
using System.Runtime.Serialization;

namespace RecShark.Extensions
{
    public static class ConvertExtensions
    {
        public static object ConvertSmart(object value)
        {
            if (double.TryParse(value.ToString(), out var doubleValue))
                return doubleValue;

            if (bool.TryParse(value.ToString(), out var boolValue))
                return boolValue;

            return value;
        }

        public static T ConvertTo<T>(this object value)
        {
            try
            {
                var underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                return (T) Convert.ChangeType(value, underlyingType);
            }
            catch (Exception)
            {
                throw new ConverterException($"Can't convert '{value}' to type {typeof(T).Name}");
            }
        }

        public static T ConvertSafelyTo<T>(this object value, T defaultValue = default(T))
        {
            value.TryConvertTo(out var result, defaultValue);
            return result;
        }

        public static bool TryConvertTo<T>(this object value, out T result, T defaultValue = default)
        {
            try
            {
                result = ConvertTo<T>(value);
                return true;
            }
            catch (ConverterException)
            {
                result = defaultValue;
                return false;
            }
        }
    }

    public class ConverterException : Exception
    {
        public ConverterException(string format, params object[] parameters)
            : base(string.Format(format, parameters))
        {
        }

        protected ConverterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
