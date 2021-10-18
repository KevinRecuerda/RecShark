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

namespace RecShark.AspNetCore.Configurator
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
            services.AddSwaggerGen()
                .AddOA3SwaggerOptions<ConfigureSwaggerOptions>();
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
            uiOptions.ConfigObject.AdditionalItems["apiCode"] = apiInfo.Code;
            uiOptions.ConfigObject.AdditionalItems["useUnsafeMarkdown"] = true;

            LoadAssemblyResources(typeof(SwaggerConfigurator), uiOptions);
        }

        /// <summary> add custom js/css </summary>
        public static void LoadAssemblyResources(Type type, SwaggerUIOptions uiOptions)
        {
            var assembly = type.Assembly;
            var resources = assembly.GetManifestResourceNames();
            foreach (var name in resources)
            {
                var stream = assembly.GetManifestResourceStream(name);
                if (stream == null)
                    continue;

                using var reader = new StreamReader(stream);
                var contentType = name.Split(".").Last() == "js" ? "script" : "style";
                uiOptions.HeadContent += $"<{contentType}>{reader.ReadToEnd()}</{contentType}>";
            }
        }

        public class ConfigureSwaggerOptions : ConfigureSwaggerVersions
        {
            public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, ApiInfo apiInfo) : base(provider, apiInfo)
            {
            }

            public override void Configure(SwaggerGenOptions options)
            {
                ConfigureSwaggerGen(options);
                base.Configure(options);
            }

            public override void Configure(SwaggerUIOptions uiOptions)
            {
                ConfigureSwaggerUi(uiOptions, ApiInfo);
                base.Configure(uiOptions);
            }
        }

        public class ConfigureSwaggerVersions : IConfigureOptions<SwaggerGenOptions>, IConfigureOptions<SwaggerUIOptions>
        {
            private readonly IApiVersionDescriptionProvider provider;
            protected readonly ApiInfo ApiInfo;

            public ConfigureSwaggerVersions(IApiVersionDescriptionProvider provider, ApiInfo apiInfo)
            {
                this.provider = provider;
                ApiInfo = apiInfo;
            }

            public virtual void Configure(SwaggerGenOptions options)
            {
                options.SwaggerGeneratorOptions.SwaggerDocs = provider.ApiVersionDescriptions
                    .OrderByDescending(x => x.ApiVersion)
                    .ToDictionary(x => x.GroupName, BuildApiInfo);
            }

            public virtual void Configure(SwaggerUIOptions uiOptions)
            {
                uiOptions.ConfigObject.Urls = provider.ApiVersionDescriptions
                    .OrderByDescending(x => x.ApiVersion)
                    .Select(x =>
                    {
                        var name = x.GroupName;
                        if (x.IsDeprecated)
                            name += " - deprecated";
                        return new UrlDescriptor() { Name = name, Url = $"/swagger/{x.GroupName}/swagger.json" };
                    })
                    .ToList();
            }

            protected virtual OpenApiInfo BuildApiInfo(ApiVersionDescription description)
            {
                var version = description.IsDeprecated
                    ? description.ApiVersion.MajorVersion + ".0"
                    : ApiInfo.Version;

                var info = new OpenApiInfo
                {
                    Title = ApiInfo.Title,
                    Version = version,
                    Description = ApiInfo.Description,
                    Contact = ApiInfo.Contact,
                    Extensions =
                    {
                        ["x-versionDeprecated"] = new OpenApiBoolean(description.IsDeprecated),
                        ["x-health"] = new OpenApiString("/health"),
                        ["x-code"] = new OpenApiString(ApiInfo.Code)
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