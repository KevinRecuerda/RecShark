using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RecShark.Extensions
{
    public static class StringExtensions
    {
        public static bool MatchAny(this string text, params string[] patterns)
        {
            return patterns.Any(p => Regex.IsMatch(text, p, RegexOptions.Singleline));
        }

        public static bool SmartMatchAny(this string text, params string[] wildcardPatterns)
        {
            string Build(string pattern)
            {
                var smartPattern = (pattern ?? "*").Replace("*", ".*")
                                                   .Replace("?", ".");
                return $"^{smartPattern}$";
            }

            var smartPatterns = wildcardPatterns.Select(Build).ToArray();
            return text.MatchAny(smartPatterns);
        }

        public static int IndexOfN(this string text, char value, int n)
        {
            if (n <= 0)
                throw new ArgumentException("n should be > 0", nameof(n));

            var offset = -1;
            for (var i = 0; i < n; i++)
            {
                offset = text.IndexOf(value, offset + 1);
                if (offset == -1)
                    return -1;
            }

            return offset;
        }

        public static string Prefixing(this string text, string prefix, bool spacing = true)
        {
            var space = spacing ? " " : "";
            return !prefix.IsNullOrEmpty() ? $"{prefix}{space}{text}" : text;
        }

        public static string Suffixing(this string text, string suffix)
        {
            return !suffix.IsNullOrEmpty() ? $"{text} {suffix}" : text;
        }

        public static string Quoting(this object value, string quotes = "'")
        {
            return $"{quotes}{value}{quotes}";
        }

        public static string Keying(this object value, string key, bool quoteValue = false)
        {
            var quotes = quoteValue ? "'" : "";
            return value.Keying(key, quotes);
        }

        public static string Keying(this object value, string key, string quotes)
        {
            return !value.ToString().IsNullOrEmpty()
                ? $"{key}={value.Quoting(quotes)}"
                : "";
        }

        public static string Tag(this string text, string tag)
        {
            return !tag.IsNullOrEmpty() ? $"[{tag}] {text}" : text;
        }

        public static string TagKey(this string text, string key, object value)
        {
            var kv = value.Keying(key);
            return text.Tag(kv);
        }

        public static string TagKeys(this string text, params (string key, object value)[] patterns)
        {
            var kvs = patterns.Select(p => p.value.Keying(p.key))
                              .Where(kv => !kv.IsNullOrEmpty())
                              .ToString("|");
            return text.Tag(kvs);
        }

        public static string TagSemantic(this string text, string name, string value)
        {
            return !value.IsNullOrEmpty() ? $"[{{{name}}}] {text}" : $"{{{name}}}{text}";
        }

        public static string Indent(this string text, bool useSpaces = true)
        {
            var tabulation = useSpaces ? "    " : "\t";
            var result     = text.Replace(Environment.NewLine, $"{Environment.NewLine}{tabulation}");
            return $"{tabulation}{result}";
        }
    }
}
