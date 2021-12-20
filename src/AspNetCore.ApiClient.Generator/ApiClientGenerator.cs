namespace RecShark.AspNetCore.ApiClient.Generator
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using NJsonSchema.CodeGeneration.CSharp;
    using NSwag;
    using NSwag.CodeGeneration.CSharp;
    using NSwag.CodeGeneration.OperationNameGenerators;

    public class ApiClientGenerator
    {
        public void Generate(string apiName, string swaggerDocsUrl, bool singleClient = false)
        {
            Console.WriteLine($"Generating client code for {apiName} api ...");
            var serverDocument = this.CreateServerDocument(swaggerDocsUrl).Result;
            this.GenerateClientCode(apiName, serverDocument, singleClient);
            Console.WriteLine($"Code generation is done for {apiName} api");
        }

        protected virtual async Task<OpenApiDocument> CreateServerDocument(string swaggerDocsUrl)
        {
            if (swaggerDocsUrl.StartsWith("http"))
                return await OpenApiDocument.FromUrlAsync(swaggerDocsUrl);

            var data = File.ReadAllText(swaggerDocsUrl);
            return await OpenApiDocument.FromJsonAsync(data);
        }

        private void GenerateClientCode(string apiName, OpenApiDocument serverDocument, bool singleClient)
        {
            var clientSettings = this.BuildSettings(apiName, singleClient);

            var clientGenerator = new CSharpClientGenerator(serverDocument, clientSettings);
            var code            = clientGenerator.GenerateFile();

            code = this.FixGeneratedClient(code);

            var       path   = this.GetOutputPath(apiName);
            using var writer = new StreamWriter(path);
            writer.Write(code);
        }

        protected virtual CSharpClientGeneratorSettings BuildSettings(string apiName, bool singleClient)
        {
            var clientSettings = new CSharpClientGeneratorSettings()
            {
                ClassName                   = $"{apiName}{{controller}}ApiClient",
                GenerateClientInterfaces    = true,
                UseHttpClientCreationMethod = false,
                InjectHttpClient            = true,
                GenerateExceptionClasses    = true,
                UseBaseUrl                  = false,
                OperationNameGenerator      = new MultipleClientsFromFirstTagAndPathSegmentsOperationNameGenerator()
            };
            if (singleClient)
                clientSettings.ClassName = $"{apiName}ApiClient";

            clientSettings.CSharpGeneratorSettings.ClassStyle = CSharpClassStyle.Poco;
            clientSettings.CSharpGeneratorSettings.Namespace  = $"{apiName}";

            const string dateType = "System.DateTime";
            clientSettings.CSharpGeneratorSettings.DateTimeType = dateType;
            clientSettings.CSharpGeneratorSettings.DateType     = dateType;

            const string listType = "System.Collections.Generic.List";
            clientSettings.CSharpGeneratorSettings.ArrayType         = listType;
            clientSettings.CSharpGeneratorSettings.ArrayBaseType     = listType;
            clientSettings.CSharpGeneratorSettings.ArrayInstanceType = listType;
            clientSettings.ResponseArrayType                         = listType;

            return clientSettings;
        }

        protected virtual string GetOutputPath(string apiName)
        {
            return $"../../../../Client/{apiName}Client.Generated.cs";
        }

        protected virtual string FixGeneratedClient(string code)
        {
            return code;
        }
    }
}
