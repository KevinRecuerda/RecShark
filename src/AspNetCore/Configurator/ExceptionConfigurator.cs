using System;
using System.Linq;
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
using RecShark.Extensions;

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

        public class ProblemsDetails : ProblemDetails
        {
            public ProblemDetails[] Problems { get; set; }

            public ProblemsDetails(Exception except, HttpContext ctx, HttpStatusCode statusCodes)
            {
                Detail = except.Message;
                Instance = ctx.Request.Path;
                Status = (int)statusCodes;
                Type = except.HelpLink;
                Title = except!.GetType().Name;
            }
           
        }
        
        public class ExceptionHandler
        {
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
                var defaultStatusCode = HttpStatusCode.InternalServerError;

                logger.LogError(exception, exception!.Message);
                var responseData = BuildProblemsDetails(exception, context);
                context.Response.ContentType = "application/problem+json; charset=utf-8";
                context.Response.StatusCode = responseData.Status ?? (int)defaultStatusCode;
                var result = JsonConvert.SerializeObject(responseData);
                await context.Response.WriteAsync(result);
            }

            private ProblemsDetails BuildProblemsDetails(Exception exception, HttpContext context)
            {
                ProblemDetails[] problems = null;
                var except = exception;

                if (!option.SkipAggregateException && exception is AggregateException)
                    problems = (exception as AggregateException).InnerExceptions
                        .Select(innerExcept => (ProblemDetails) new ProblemsDetails(innerExcept, context, GetStatusCode(innerExcept))).ToArray();
                else
                    except = exception.InnerException ?? except;
                
                var code = GetStatusCode(except);
                var responseData = new ProblemsDetails(except, context, code) { Problems = problems };
                return responseData;
            }

            private HttpStatusCode GetStatusCode(Exception exception)
            {
                var type = exception!.GetType();

                return option.ExceptionStatusCodes.GetSafely(type, HttpStatusCode.InternalServerError);
            }
        }
    }
}