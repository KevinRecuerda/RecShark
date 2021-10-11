using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Swashbuckle.AspNetCore.Annotations;

namespace RecShark.AspNetCore.Extensions.Extensions
{
    public static class SwaggerTagBadgeDefault
    {
        [AttributeUsage(AttributeTargets.Class)]
        public class DataAttribute : SwaggerTagBadgeAttribute
        {
            public DataAttribute() : base("DATA", 10, BadgeColor.CopperRust) { }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class FuncAttribute : SwaggerTagBadgeAttribute
        {
            public FuncAttribute() : base("FUNC", 20, BadgeColor.EastBay) { }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class ReportingAttribute : SwaggerTagBadgeAttribute
        {
            public ReportingAttribute() : base("REPORTING", 30, BadgeColor.FadedJade) { }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class MonitoringAttribute : SwaggerTagBadgeAttribute
        {
            public MonitoringAttribute() : base("MONITORING", 80, BadgeColor.FadedJade) { }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class TechAttribute : SwaggerTagBadgeAttribute
        {
            public TechAttribute() : base("TECH", 99, BadgeColor.Mako) { }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SwaggerTagBadgeAttribute : SwaggerTagAttribute
    {
        public SwaggerTagBadgeAttribute(string name)
            : this(name, 0) { }

        public SwaggerTagBadgeAttribute(string name, int order)
            : this(name, order, BadgeColorCache.GetColor(name)) { }

        public SwaggerTagBadgeAttribute(string name, int order, BadgeColor color)
            : base(BuildDescription(name, order, color))
        {
            this.Name  = name;
            this.Order = order;
            this.Color = color;
        }

        public string Name { get; }

        public int Order { get; }

        public BadgeColor Color { get; }

        private static string BuildDescription(string name, int order, BadgeColor color)
        {
            return $"<div order='{order}-{name}' class='badge badge-discreet-{ToKebabCase(color.ToString())} badge-prepend-square'>{name}</div>";
        }

        private static string ToKebabCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return Regex.Replace(
                             value,
                             "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                             "-$1",
                             RegexOptions.Compiled)
                        .Trim()
                        .ToLower();
        }
    }

    public enum BadgeColor
    {
        Mako,
        CopperRust,
        EastBay,
        FadedJade
    }

    public static class BadgeColorCache
    {
        private static readonly Dictionary<string, BadgeColor> Cache = new Dictionary<string, BadgeColor>();

        private static readonly BadgeColor[] Colors = (BadgeColor[])Enum.GetValues(typeof(BadgeColor));

        public static BadgeColor GetColor(string description)
        {
            if (!Cache.ContainsKey(description))
                Cache[description] = Colors[Cache.Count % Colors.Length];

            return Cache[description];
        }
    }
}
