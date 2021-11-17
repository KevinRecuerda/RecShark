using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RecShark.AspNetCore.Configurator;
using RecShark.AspNetCore.Options;

namespace RecShark.AspNetCore.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var apiInfo = Configuration.GetSection("ApiInfo").Get<SwaggerConfigurator.ApiInfo>();
            services.AddSingleton(apiInfo);

            services.AddOptions()
                    .AddHttpContextAccessor()
                    .AddOA3Routing()
                    .AddMonitoring()
                     // .AddSecurity()
                    .AddOA3Swagger()
                    .AddOA3Mvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting()
               .UseException(new ExceptionOption() { SkipAggregateException = true })
               // .UseSecurity()
               .UseOA3Swagger()
               .UseEndpoints(
                    endpoints =>
                    {
                        endpoints.MapHealthChecks();
                        endpoints.MapControllers();
                    });
        }
    }
}
