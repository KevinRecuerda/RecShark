namespace RecShark.AspNetCore.Extensions
{
    using RecShark.AspNetCore.Configurator;
    using Serilog;
    using Serilog.Configuration;

    public static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration ExcludePaths(this LoggerFilterConfiguration filter, params string[] excludedPaths)
        {
            return filter.With(new LoggingConfigurator.ExcludedPathFilter(excludedPaths));
        }
    }
}