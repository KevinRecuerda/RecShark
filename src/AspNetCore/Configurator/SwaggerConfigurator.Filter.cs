using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NJsonSchema.Converters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RecShark.AspNetCore.Configurator
{
    using IDocumentFilter = Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter;
    using ISchemaFilter = Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter;

    public static partial class SwaggerConfigurator
    {
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
