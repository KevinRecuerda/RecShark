namespace RecShark.AspNetCore.Sample
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using RecShark.AspNetCore.Configurator;
    using RecShark.AspNetCore.Extensions;
    using RecShark.AspNetCore.Options;
    using RecShark.DependencyInjection;
    using Swashbuckle.AspNetCore.SwaggerUI;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
        {
            services.AddConfig<SwaggerConfigurator.ApiInfo>("ApiInfo");
            services.AddHostedService<ShortStartupHealthService>();
            services.AddHostedService<FailStartupHealthService>();
            services.AddHostedService<FailAsyncStartupHealthService>();
            services.AddHostedService<CanceledStartupHealthService>();

            services.AddOptions()
                    .AddHttpContextAccessor()
                    .AddOA3Routing()
                    .AddMonitoring(this.Configuration, loggerConfigurator: config => config.Filter.ExcludePaths("/api/v2/enum"))
                     // .AddSecurity()
                    .AddException(env, new ExceptionOption() {SkipAggregateException = false})
                    .AddOA3Swagger<CustomSwaggerOptions>()
                    .AddOA3Mvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting()
               .UseMonitoring(applicationLifetime)
               .UseException()
                // .UseSecurity()
               .UseOA3Swagger()
               .UseEndpoints(
                    endpoints =>
                    {
                        endpoints.MapConventionalHealthChecks();
                        endpoints.MapControllers();
                    });
        }

        public class CustomSwaggerOptions : SwaggerConfigurator.ConfigureSwaggerOptions
        {
            public CustomSwaggerOptions(IApiVersionDescriptionProvider provider, SwaggerConfigurator.ApiInfo apiInfo) : base(provider, apiInfo) { }

            public override void Configure(SwaggerUIOptions uiOptions)
            {
                base.Configure(uiOptions);
                uiOptions.UseLogo("logo.png");
                uiOptions.UseBackground("bg.jpg");
            }
        }
    }
}