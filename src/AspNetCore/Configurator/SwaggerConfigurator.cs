using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.Swagger;

namespace RecShark.AspNetCore.Configurator
{
    public static partial class SwaggerConfigurator
    {
        public static IApplicationBuilder UseOA3Swagger(this IApplicationBuilder app)
        {
            app.UseSwagger()
               .UseSwaggerUI();

            return app;
        }

        public static IServiceCollection AddOA3Swagger(this IServiceCollection services)
        {
            return services.AddOA3Swagger<ConfigureSwaggerOptions>();
        }

        public static IServiceCollection AddOA3Swagger<T>(this IServiceCollection services)
            where T : class, IConfigureOptions<SwaggerGenOptions>, IConfigureOptions<SwaggerUIOptions>
        {
            services.AddSwaggerGen()
                    .AddOA3SwaggerOptions<T>();
            return services;
        }

        public static void AddOA3SwaggerOptions<T>(this IServiceCollection services)
            where T : class, IConfigureOptions<SwaggerGenOptions>, IConfigureOptions<SwaggerUIOptions>
        {
            services.AddSwaggerGenNewtonsoftSupport();
            services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, T>();
            services.AddTransient<IConfigureOptions<SwaggerUIOptions>, T>();
        }

        public static void ConfigureSwaggerGen(SwaggerGenOptions options)
        {
            options.CustomOperationIds(IdGenerator.Operation);
            options.CustomSchemaIds(IdGenerator.Schema);

            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#swashbuckleaspnetcoreannotations
            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/README.md#enrich-polymorphic-base-classes-with-discriminator-metadata
            options.EnableAnnotations(enableAnnotationsForInheritance: false, enableAnnotationsForPolymorphism: false);

            InheritanceConfigurator.UseInheritance(options);
            InheritanceConfigurator.UsePolymorphism(options);

            options.UseAllOfToExtendReferenceSchemas();
            options.SchemaFilter<RequiredPropertiesSchemaFilter>();

            // https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters
            options.ExampleFilters();

            options.DocumentFilter<OrderTagsDocumentFilter>();
        }

        public static void ConfigureSwaggerUi(SwaggerUIOptions uiOptions, ApiInfo apiInfo)
        {
            uiOptions.DocumentTitle = $"{apiInfo.Code} | swagger";
            uiOptions.DocExpansion(DocExpansion.None);
            uiOptions.DefaultModelExpandDepth(-1);
            uiOptions.DefaultModelsExpandDepth(-1);
            uiOptions.EnableDeepLinking();
            uiOptions.DisplayRequestDuration();
            uiOptions.ConfigObject.AdditionalItems["apiCode"]           = apiInfo.Code;
            uiOptions.ConfigObject.AdditionalItems["useUnsafeMarkdown"] = true;

            LoadAssemblyResources(typeof(SwaggerConfigurator), uiOptions);
        }

        /// <summary> add custom js/css </summary>
        public static void LoadAssemblyResources(Type type, SwaggerUIOptions uiOptions)
        {
            var assembly  = type.Assembly;
            var resources = assembly.GetManifestResourceNames();
            foreach (var name in resources)
            {
                var stream = assembly.GetManifestResourceStream(name);
                if (stream == null)
                    continue;

                using var reader      = new StreamReader(stream);
                var       contentType = name.Split(".").Last() == "js" ? "script" : "style";
                uiOptions.HeadContent += $"<{contentType}>{reader.ReadToEnd()}</{contentType}>";
            }
        }

        public static class IdGenerator
        {
            public static string Operation(ApiDescription apiDesc)
            {
                var regex    = new Regex(@"\{(\w+)\}");
                var safePath = regex.Replace(apiDesc.RelativePath, "by-$1");

                var items = safePath.Split("/").ToList();
                items.Add(apiDesc.HttpMethod.ToLower());

                if (items[0] == "api")
                    items.RemoveAt(0);

                var id = string.Join(".", items);
                return id;
            }

            public static string Schema(Type type)
            {
                // this handle correctly generic types
                return type.FriendlyId();
            }
        }

        public class ApiInfo : OpenApiInfo
        {
            public string Code { get; set; }
        }
    }
}
