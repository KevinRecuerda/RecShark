using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace RecShark.AspNetCore.Configurator
{
    public static partial class SwaggerConfigurator
    {
        public class ConfigureSwaggerOptions : ConfigureSwaggerVersions
        {
            public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, ApiInfo apiInfo) : base(provider, apiInfo) { }

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
            private readonly   IApiVersionDescriptionProvider provider;
            protected readonly ApiInfo                        ApiInfo;

            public ConfigureSwaggerVersions(IApiVersionDescriptionProvider provider, ApiInfo apiInfo)
            {
                this.provider = provider;
                ApiInfo       = apiInfo;
            }

            public virtual void Configure(SwaggerGenOptions options)
            {
                options.SwaggerGeneratorOptions.SwaggerDocs = provider.ApiVersionDescriptions
                                                                      .OrderByDescending(x => x.ApiVersion)
                                                                      .ToDictionary(x => x.GroupName, BuildApiInfo);
            }

            public virtual void Configure(SwaggerUIOptions uiOptions)
            {
                uiOptions.ConfigObject.Urls =
                    provider.ApiVersionDescriptions
                            .OrderByDescending(x => x.ApiVersion)
                            .Select(
                                 x =>
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
                    Title       = ApiInfo.Title,
                    Version     = version,
                    Description = ApiInfo.Description,
                    Contact     = ApiInfo.Contact,
                    Extensions =
                    {
                        ["x-versionDeprecated"] = new OpenApiBoolean(description.IsDeprecated),
                        ["x-health"]            = new OpenApiString("/health"),
                        ["x-code"]              = new OpenApiString(ApiInfo.Code),
                        ["x-env"]               = new OpenApiString(ApiInfo.Env),
                    }
                };

                info.Description += $@"
> [api health](/health)
";
                return info;
            }
        }
    }
}
