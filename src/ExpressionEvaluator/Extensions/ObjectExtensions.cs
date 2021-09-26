using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;

namespace RecShark.ExpressionEvaluator.Extensions
{
    public static class ObjectExtensions
    {
        public static string Default { get; } = "";

        public static double AsDouble(this object value, ParserRuleContext context = null)
        {
            return As(value, context, TryAsDouble);
        }

        public static bool AsBool(this object value, ParserRuleContext context = null)
        {
            return As(value, context, TryAsBool);
        }

        public static string AsString(this object value, ParserRuleContext context = null)
        {
            return As(value, context, TryAsString);
        }

        public static List<object> AsList(this object value, ParserRuleContext context = null)
        {
            return As(value, context, TryAsList);
        }

        private static T As<T>(object value, ParserRuleContext context, Func<object, (bool, T)> converter, [CallerMemberName] string method = "")
        {
            var (success, convertedValue) = converter(value);
            if (success)
                return convertedValue;

            var contextMessage = context != null ? $" (for '{context.Start.InputStream}')" : string.Empty;
            throw new EvaluationException($"Cannot convert '{value}' to {method.Substring(2).ToLower()}{contextMessage}");
        }

        private static (bool, double) TryAsDouble(object value)
        {
            switch (value)
            {
                case double doubleValue:                             return (true, doubleValue);
                case int intValue:                                   return (true, intValue);
                case decimal decimalValue:                           return (true, decimal.ToDouble(decimalValue));
                case string stringValue when stringValue == Default: return (true, 0);
            }

            return (false, default);
        }

        private static (bool, bool) TryAsBool(object value)
        {
            switch (value)
            {
                case bool boolValue:                                 return (true, boolValue);
                case string stringValue when stringValue == Default: return (true, false);
            }

            return (false, default);
        }

        private static (bool, string) TryAsString(object value)
        {
            switch (value)
            {
                case string stringValue: return (true, stringValue);
            }

            return (false, default);
        }

        private static (bool, List<object>) TryAsList(object value)
        {
            switch (value)
            {
                case List<object> list:                              return (true, list);
                case string stringValue when stringValue == Default: return (true, new List<object>());
            }

            return (false, default);
        }
    }
}