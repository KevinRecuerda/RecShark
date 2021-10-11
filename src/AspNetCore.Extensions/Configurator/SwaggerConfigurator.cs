using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NJsonSchema.Converters;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.Swagger;

namespace RecShark.AspNetCore.Extensions.Configurator
{
    using IDocumentFilter = Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter;
    using ISchemaFilter = Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter;

    public static class SwaggerConfigurator
    {
        public static IApplicationBuilder UseOA3Swagger(this IApplicationBuilder app)
        {
            app.UseSwagger()
                .UseSwaggerUI();

            return app;
        }

        public static IServiceCollection AddOA3Swagger(this IServiceCollection services)
        {
            services.AddSwaggerGen();
            services.AddSwaggerGenNewtonsoftSupport();
            services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddTransient<IConfigureOptions<SwaggerUIOptions>, ConfigureSwaggerOptions>();
            return services;
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
            uiOptions.ConfigObject.AdditionalItems["apiCode"] = apiInfo.Code;
            uiOptions.ConfigObject.AdditionalItems["useUnsafeMarkdown"] = true;

            // Add custom js/css
            var assembly = typeof(SwaggerConfigurator).Assembly;
            var resources = assembly.GetManifestResourceNames();
            foreach (var name in resources)
            {
                var stream = assembly.GetManifestResourceStream(name);
                if (stream == null)
                    continue;

                using var reader = new StreamReader(stream);
                var type = name.Split(".").Last() == "js" ? "script" : "style";
                uiOptions.HeadContent += $"<{type}>{reader.ReadToEnd()}</{type}>";
            }
        }

        public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>, IConfigureOptions<SwaggerUIOptions>
        {
            private readonly IApiVersionDescriptionProvider provider;
            private readonly ApiInfo apiInfo;

            public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IOptions<ApiInfo> apiInfo)
            {
                this.provider = provider;
                this.apiInfo = apiInfo.Value;
            }

            public void Configure(SwaggerGenOptions options)
            {
                ConfigureSwaggerGen(options);
                foreach (var description in this.provider.ApiVersionDescriptions)
                    options.SwaggerGeneratorOptions.SwaggerDocs[description.GroupName] = this.BuildApiInfo(description);
            }

            public void Configure(SwaggerUIOptions uiOptions)
            {
                ConfigureSwaggerUi(uiOptions, this.apiInfo);
                foreach (var description in this.provider.ApiVersionDescriptions.OrderByDescending(x => x.ApiVersion))
                {
                    var name = description.GroupName;
                    if (description.IsDeprecated)
                        name += " - deprecated";
                    uiOptions.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", name);
                }
            }

            protected OpenApiInfo BuildApiInfo(ApiVersionDescription description)
            {
                var version = description.IsDeprecated
                    ? description.ApiVersion.MajorVersion + ".0"
                    : this.apiInfo.Version;

                var info = new OpenApiInfo
                {
                    Title = this.apiInfo.Title,
                    Version = version,
                    Description = this.apiInfo.Description,
                    Contact = this.apiInfo.Contact,
                    Extensions =
                    {
                        ["x-versionDeprecated"] = new OpenApiBoolean(description.IsDeprecated),
                        ["x-health"] = new OpenApiString("/health"),
                        ["x-code"] = new OpenApiString(this.apiInfo.Code)
                    }
                };

                info.Description += $@"
> [api health](/health)
";
                return info;
            }
        }

        public class ApiInfo : OpenApiInfo
        {
            public string Code { get; set; }
        }

        public static class IdGenerator
        {
            public static string Operation(ApiDescription apiDesc)
            {
                var regex = new Regex(@"\{(\w+)\}");
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

        /// <summary> This filter aims to order tags. </summary>
        public class OrderTagsDocumentFilter : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                swaggerDoc.Tags = swaggerDoc.Tags.OrderBy(GetOrder).ToList();
            }

            public static string GetOrder(OpenApiTag tag)
            {
                var order = Regex.Match(tag.Description, "order='(.*)' ").Groups[1].Value;
                return $"{order}-{tag.Name}";
            }
        }

        /// <summary> This filter aims to manage required properties. </summary>
        public class RequiredPropertiesSchemaFilter : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                schema.Required = schema.Properties
                    .Where(p => !p.Value.Nullable)
                    .Select(p => p.Key)
                    .ToHashSet();
            }
        }

        public static class InheritanceConfigurator
        {
            private static readonly JsonInheritanceConverter InheritanceConverter = new JsonInheritanceConverter();

            public static void UseInheritance(SwaggerGenOptions options)
            {
                options.UseAllOfForInheritance();
            }

            public static void UsePolymorphism(SwaggerGenOptions options)
            {
                options.UseOneOfForPolymorphism();

                options.SelectSubTypesUsing(
                    baseType =>
                    {
                        var subTypes = baseType.GetCustomAttributes(typeof(KnownTypeAttribute), false)
                            .Cast<KnownTypeAttribute>()
                            .Select(x => x.Type)
                            .ToList();

                        // hack to have base class at first (even if abstract), in order to have a relevant code gen
                        if (subTypes.Any() && baseType.IsAbstract)
                            subTypes.Insert(0, baseType);
                        return subTypes;
                    });
                options.SelectDiscriminatorNameUsing(baseType => InheritanceConverter.DiscriminatorName);
                options.SelectDiscriminatorValueUsing(subType => InheritanceConverter.GetDiscriminatorValue(subType));

                options.SchemaFilter<PolymorphismSchemaFilter>();
            }

            private class PolymorphismSchemaFilter : ISchemaFilter
            {
                public void Apply(OpenApiSchema schema, SchemaFilterContext context)
                {
                    if (schema.Properties.ContainsKey(InheritanceConverter.DiscriminatorName))
                        schema.Discriminator = new OpenApiDiscriminator() { PropertyName = InheritanceConverter.DiscriminatorName };
                }
            }
        }
    }
}