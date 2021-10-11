using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecShark.AspNetCore.Extensions.Options;

namespace RecShark.AspNetCore.Extensions.Configurator
{
    public static class ExceptionConfigurator
    {
        public static IApplicationBuilder UseException(this IApplicationBuilder app, ExceptionOption option)
        {
            app.UseExceptionHandler(builder =>
            {
                var logger = builder.ApplicationServices.GetService<ILogger<ExceptionHandler>>();
                var handler = new ExceptionHandler(option, logger);
                builder.Run(handler.Run);
            });
            return app;
        }

        public class ExceptionHandler
        {
            private static readonly ErrorResponse DefaultResponse = new ErrorResponse("InternalServerError", "Internal server error");

            private readonly ExceptionOption option;
            private readonly ILogger<ExceptionHandler> logger;

            public ExceptionHandler(ExceptionOption option, ILogger<ExceptionHandler> logger)
            {
                this.option = option;
                this.logger = logger;
            }

            public async Task Run(HttpContext context)
            {
                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = feature.Error;

                if (option.SkipAggregateException)
                {
                    while (exception is AggregateException)
                        exception = exception.InnerException;
                }

                logger.LogError(exception, exception!.Message);

                var error = DefaultResponse;
                var code = HttpStatusCode.InternalServerError;
                var type = exception!.GetType();

                if (option.ExceptionStatusCodes.ContainsKey(type))
                {
                    code = option.ExceptionStatusCodes[type];
                    error = new ErrorResponse(type.Name, exception.Message);
                }

                var result = JsonConvert.SerializeObject(error);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)code;
                await context.Response.WriteAsync(result);
            }
        }

        public class ErrorResponse
        {
            public ErrorResponse(string code, string message)
            {
                Code = code;
                Message = message;
            }

            public string Code { get; }
            public string Message { get; }
        }
    }
}