using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecShark.AspNetCore.Options;

namespace RecShark.AspNetCore.Configurator
{
    public static class ExceptionConfigurator
    {
        public static IApplicationBuilder UseException(this IApplicationBuilder app)
        {
            app.UseProblemDetails();
            return app;
        }

        public static IServiceCollection AddException(this IServiceCollection services, IWebHostEnvironment env, ExceptionOption option)
        {
            return services.AddProblemDetails((setup) =>
            {
                var problemHandler = new ProblemHandler(option, setup);

                problemHandler.Build();
                setup.IncludeExceptionDetails = (ctx, ex) => env.IsDevelopment();

            }).AddProblemDetailsConventions();
        }

        public class ProblemHandler
        {
            private readonly ExceptionOption option;
            private readonly ProblemDetailsOptions setup;
            
            public ProblemHandler(ExceptionOption option,  ProblemDetailsOptions setup)
            {
                this.option = option;
                this.setup = setup;
            }

            public void Build()
            {
                BuildStatusCodeMapping();
                BuildMultiProblemMapping();
            }

            private void BuildStatusCodeMapping()
            {
                MethodInfo method = typeof(ProblemDetailsOptions).GetMethod("MapToStatusCode");

                foreach (var exceptionStatusCode in option.ExceptionStatusCodes)
                {
                    var key = exceptionStatusCode.Key;
                    var value = exceptionStatusCode.Value;
                    MethodInfo genericMethod = method.MakeGenericMethod(key);
                    genericMethod.Invoke(setup, new object?[] {(int)value});
                }
            }

            private void BuildMultiProblemMapping()
            {
                setup.Map<AggregateException>((ctx, except) =>
                {
                    var multiProblemDetail = new MultiProblemDetail()
                    {
                        Detail = except.InnerException?.Message ?? except.Message,
                        Instance = ctx.Request.Path,
                        Status = ctx.Response.StatusCode,
                        Type = except.HelpLink,
                        Title = except!.GetType().Name,
                    };

                    if (!option.SkipAggregateException)
                    {
                        multiProblemDetail.errors = except.InnerExceptions.Select(innerExcept => new ProblemDetails()
                        {
                            Detail = innerExcept.InnerException?.Message ?? except.Message,
                            Instance = ctx.Request.Path,
                            Status = ctx.Response.StatusCode,
                            Type = innerExcept.HelpLink,
                            Title = innerExcept!.GetType().Name,
                        }).ToArray();
                    }
                    return multiProblemDetail;
                });
               
            }
        }

        public class MultiProblemDetail : ProblemDetails
        {
            public ProblemDetails[] errors { get; set; }
        }
    }
}