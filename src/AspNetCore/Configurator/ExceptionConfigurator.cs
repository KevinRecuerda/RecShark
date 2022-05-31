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

        public static IServiceCollection AddException(this IServiceCollection services, ExceptionOption option)
        {
            return services.AddProblemDetails((setup) => new ProblemDetailsBuilder(option, setup).Build()).AddProblemDetailsConventions();
        }

        public class ProblemDetailsBuilder
        {
            private readonly ExceptionOption exceptionOptions;
            private readonly ProblemDetailsOptions problemDetailsOption;
            
            public ProblemDetailsBuilder(ExceptionOption exceptionOptions,  ProblemDetailsOptions problemDetailsOption)
            {
                this.exceptionOptions = exceptionOptions;
                this.problemDetailsOption = problemDetailsOption;
            }

            public void Build()
            {
                BuildStatusCodes();
                BuildAggregatedProblemDetails();
                problemDetailsOption.Rethrow<Exception>();
                problemDetailsOption.IncludeExceptionDetails = (ctx, ex) =>
                {
                    var env = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>();
                    return env.IsDevelopment();
                };
            }

            private void BuildStatusCodes()
            {
                MethodInfo mappingMethod = typeof(ProblemDetailsOptions).GetMethod("MapToStatusCode");

                foreach (var exceptionStatusCode in exceptionOptions.ExceptionStatusCodes)
                {
                    var exceptionType = exceptionStatusCode.Key;
                    var statusCode = exceptionStatusCode.Value;
                    MethodInfo genericMethod = mappingMethod?.MakeGenericMethod(exceptionType);
                    
                    genericMethod?.Invoke(problemDetailsOption, new object[] {(int)statusCode});
                }
            }

            private void BuildAggregatedProblemDetails()
            {
                problemDetailsOption.Map<AggregateException>((ctx, except) =>
                {
                    Exception exception = except;
                    ProblemDetails[] errors = null;
                    
                    if (!exceptionOptions.SkipAggregateException)
                        errors = except.InnerExceptions
                                .Select(innerExcept => (ProblemDetails) ProblemsDetails.Build(innerExcept, ctx)).ToArray();
                    else
                        exception = exception.InnerException ?? except;
                    
                    ProblemsDetails problemsDetails = ProblemsDetails.Build(exception, ctx);
                    ctx.Response.StatusCode = (int)exceptionOptions.ExceptionStatusCodes[exception.GetType()];
                    problemsDetails.Errors = errors;
                    
                    return problemsDetails;
                });
               
            }
        }

        public class ProblemsDetails : ProblemDetails
        {
            public ProblemDetails[] Errors { get; set; }

            public static ProblemsDetails Build(Exception except, HttpContext ctx)
            {
                return new ProblemsDetails()
                {
                    Detail = except.Message,
                    Instance = ctx.Request.Path,
                    Status = ctx.Response.StatusCode,
                    Type = except.HelpLink,
                    Title = except!.GetType().Name,
                };
            }
        }
    }
}