namespace RecShark.AspNetCore
{
    using System.IO;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    public static class DefaultProgram
    {
        public static void Run<TStartup>(string[] args)
            where TStartup : class
        {
            CreateWebHostBuilder<TStartup>(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder<TStartup>(string[] args)
            where TStartup : class
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            var environment = builder.GetSetting("environment");

            var config = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json",                true, true)
                        .AddJsonFile($"appsettings.{environment}.json", true, true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                        .Build();

            return builder.UseConfiguration(config)
                          .UseStartup<TStartup>();
        }
    }
}
