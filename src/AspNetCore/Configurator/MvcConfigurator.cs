using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RecShark.Extensions;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace RecShark.AspNetCore.Configurator
{
    public static class MvcConfigurator
    {
        public static IMvcBuilder AddOA3MvcSecured(
            this IServiceCollection          services,
            Action<MvcOptions>               configureMvc        = null,
            Action<JsonOptions>              configureJson       = null,
            Action<MvcNewtonsoftJsonOptions> configureNewtonsoft = null)
        {
            void ConfigureMvcSecured(MvcOptions options)
            {
                options.Filters.Add(new AuthorizeFilter());
                configureMvc?.Invoke(options);
            }

            return services.AddOA3Mvc(ConfigureMvcSecured, configureJson, configureNewtonsoft);
        }

        public static IMvcBuilder AddOA3Mvc(
            this IServiceCollection          services,
            Action<MvcOptions>               configureMvc        = null,
            Action<JsonOptions>              configureJson       = null,
            Action<MvcNewtonsoftJsonOptions> configureNewtonsoft = null)
        {
            return services
                   .AddControllers(options =>
                   {
                       ConfigureMvc(options);
                       configureMvc?.Invoke(options);
                   })
                   .AddJsonOptions(options =>
                   {
                       ConfigureJson(options);
                       configureJson?.Invoke(options);
                   })
                   .ConfigureApiBehaviorOptions(options =>
                   {
                       options.InvalidModelStateResponseFactory = ctx =>
                       {
                           var defaultResult = new BadRequestObjectResult(ctx.ModelState);

                           var errors         = ((IDictionary<string, object>) defaultResult.Value).ToDictionary(x => x.Key, x => (string[]) x.Value);
                           var problemDetails = ExceptionConfigurator.CreateValidationProblemDetails("", errors);
                           return new BadRequestObjectResult(problemDetails);
                       };
                   })

                   // use newtonsoft for some specific serialization (Polymorphism, Dictionary...)
                   // https://github.com/dotnet/runtime/issues/30524#issuecomment-534386814
                   // managed in 5.0 => https://github.com/dotnet/runtime/issues/30524#issuecomment-539666802
                   .AddNewtonsoftJson(options =>
                   {
                       ConfigureNewtonsoft(options.SerializerSettings);
                       configureNewtonsoft?.Invoke(options);
                   });
        }

        public static void ConfigureMvc(MvcOptions options)
        {
            // kebab-case routing
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-3.1#use-a-parameter-transformer-to-customize-token-replacement
            options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));

            options.AllowEmptyInputInBodyModelBinding = true;
        }

        public static void ConfigureJson(JsonOptions options)
        {
            options.JsonSerializerOptions.IgnoreNullValues = true;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));

            // options.JsonSerializerOptions.DictionaryKeyPolicy  =  JsonNamingPolicy.CamelCase; => KO: rename data keys as "MY_DATA"
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }

        public static void ConfigureNewtonsoft(JsonSerializerSettings settings)
        {
            // TODO: see how to manage auto inheritance
            // TypeNameHandling should be NONE for security reason)
            // so, see with global converter
            // OR use https://github.com/dahomey-technologies/Dahomey.Json
            // options.SerializerSettings.TypeNameHandling      = TypeNameHandling.Auto;
            // options.SerializerSettings.SerializationBinder   = new BasicSerializationBinder();

            var namingStrategy = new CamelCaseNamingStrategy();

            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.NullValueHandling     = NullValueHandling.Ignore;
            settings.ContractResolver      = new DefaultContractResolver {NamingStrategy = namingStrategy};
            settings.Converters.Add(new StringEnumConverter(namingStrategy, false));
            settings.Converters.Add(new DictionaryWithEnumKeyConverter(settings));
        }
    }

    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string TransformOutbound(object value)
        {
            var text = value?.ToString();
            if (text == null)
                return null;

            return Regex.Replace(text, "([a-z])([A-Z])", "$1-$2", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100))
                        .ToLowerInvariant();
        }
    }

    public class DictionaryWithEnumKeyConverter : JsonConverter
    {
        private readonly JsonSerializerSettings settings;

        public DictionaryWithEnumKeyConverter(JsonSerializerSettings settings)
        {
            this.settings = settings;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            var dictionary = (IDictionary) value;
            foreach (DictionaryEntry entry in dictionary)
            {
                var propertyNameQuoted = JsonConvert.SerializeObject(entry.Key, settings);
                var propertyName       = propertyNameQuoted[1..^1];
                writer.WritePropertyName(propertyName);

                serializer.Serialize(writer, entry.Value);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var createdType = serializer.ContractResolver.ResolveContract(objectType).CreatedType;
            var dictionary  = (IDictionary) Activator.CreateInstance(createdType, BindingFlags.Instance | BindingFlags.Public);

            var innerTypes = objectType.GetDictionaryInnerTypes();

            var jObject = JObject.Load(reader);

            foreach (var (jKey, jValue) in jObject)
            {
                var key   = JsonConvert.DeserializeObject($"\"{jKey}\"", innerTypes[0], settings);
                var value = jValue.ToObject(innerTypes[1], serializer);
                dictionary.Add(key, value);
            }

            return dictionary;
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType.AsDictionary() == null)
                return false;

            var innerTypes = objectType.GetDictionaryInnerTypes();
            return innerTypes[0].IsEnum;
        }
    }
}
