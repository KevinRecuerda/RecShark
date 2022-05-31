using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecShark.AspNetCore.Options;

namespace RecShark.AspNetCore.Configurator
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
            private static readonly ProblemDetails DefaultResponse = new ProblemDetails()
            {
                Title = "InternalServerError",
                Detail = "Internal server error"
            };

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
                    error = new ProblemDetails
                    {
                        Status =  (int)code,
                        Type = exception.HelpLink,
                        Title = type.Name,
                        Detail = exception.Message,
                        Instance = context.Request.Path
                    };
                    context.Response.ContentType = "problem+json";
                }
                else
                    context.Response.ContentType = "application/json";
                
                var result = JsonConvert.SerializeObject(error);
                context.Response.StatusCode = (int)code;
                await context.Response.WriteAsync(result);
            }
        }
    }
}