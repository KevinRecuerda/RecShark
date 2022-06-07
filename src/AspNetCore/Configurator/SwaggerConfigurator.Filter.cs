using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NJsonSchema.Converters;
using RecShark.Extensions;
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

        /// <summary> This filter aims to manage required parameters. </summary>
        public class RequiredParametersOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var requiredParams = context.MethodInfo
                                            .GetParameters()
                                            .Where(p => !p.ParameterType.IsNullableType())
                                            .Select(p => p.Name)
                                            .ToHashSet();
                foreach (var parameter in operation.Parameters)
                {
                    if (requiredParams.Contains(parameter.Name))
                        parameter.Required = true;
                }
            }
        }

        /// <summary> This filter aims to manage ValidationProblemDetails for 400 status. </summary>
        public class ValidationProblemDetailsFilter : IOperationFilter, IDocumentFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var status400 = operation.Responses.GetValueOrDefault(StatusCodes.Status400BadRequest.ToString());
                if (status400 == null)
                    return;

                foreach (var openApiMediaType in status400.Content.Values)
                    openApiMediaType.Schema.Reference.Id = nameof(ValidationProblemDetails);
            }

            public void Apply(OpenApiDocument doc, DocumentFilterContext context)
            {
                context.SchemaGenerator.GenerateSchema(typeof(ValidationProblemDetails), context.SchemaRepository);
            }
        }

        public static class InheritanceConfigurator
        {
            private static readonly JsonInheritanceConverter           InheritanceConverter = new JsonInheritanceConverter();
            private static readonly ConcurrentDictionary<Type, Type[]> CachedSubTypes       = new ConcurrentDictionary<Type, Type[]>();

            public static void UseInheritance(SwaggerGenOptions options)
            {
                options.UseAllOfForInheritance();
            }

            public static void UsePolymorphism(SwaggerGenOptions options)
            {
                options.UseOneOfForPolymorphism();

                options.SelectSubTypesUsing(t => CachedSubTypes.GetOrAdd(t, type =>
                {
                    var subTypes = type.GetCustomAttributes(typeof(KnownTypeAttribute), false)
                                       .Cast<KnownTypeAttribute>()
                                       .Select(x => x.Type)
                                       .ToList();

                    // hack to have base class at first (even if abstract), in order to have a relevant code gen
                    if (type.IsAbstract && subTypes.Any())
                        subTypes.Insert(0, type);

                    return subTypes.ToArray();
                }));
                options.SelectDiscriminatorNameUsing(baseType => InheritanceConverter.DiscriminatorName);
                options.SelectDiscriminatorValueUsing(subType => InheritanceConverter.GetDiscriminatorValue(subType));
            }
        }
    }
}
