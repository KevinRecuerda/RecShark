using System;
using System.Collections.Generic;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Extensions;

namespace RecShark.AspNetCore.Configurator
{
    public static class ExceptionConfigurator
    {
        public static IApplicationBuilder UseException(this IApplicationBuilder app)
        {
            app.UseProblemDetails();
            return app;
        }

        public static IServiceCollection AddException(this IServiceCollection services, Action<ProblemDetailsOptions> configureProblemDetails = null)
        {
            return services.AddProblemDetails(options =>
                           {
                               options.ValidationProblemStatusCode = StatusCodes.Status400BadRequest;

                               options.Map<ArgumentException>((_, ex) =>
                               {
                                   var errors = new Dictionary<string, string[]>();
                                   if (ex.ParamName != null)
                                       errors[ex.ParamName] = new[] {ex.Message};

                                   var problemDetails = CreateValidationProblemDetails(ex.Message.Tag(ex.ParamName), errors);
                                   return problemDetails;
                               });
                               options.MapToStatusCode<UnauthorizedAccessException>(StatusCodes.Status403Forbidden);
                               options.MapToStatusCode<NotFoundException>(StatusCodes.Status404NotFound);

                               configureProblemDetails?.Invoke(options);
                           })
                           .AddProblemDetailsConventions();
        }

        public static ValidationProblemDetails CreateValidationProblemDetails(string detail, IDictionary<string, string[]> errors)
        {
            var pb = StatusCodeProblemDetails.Create(StatusCodes.Status400BadRequest);
            pb.Detail = detail;

            var validationPb = new ValidationProblemDetails(errors);
            pb.CloneTo(validationPb);
            return validationPb;
        }
    }


    public class NotFoundException : Exception
    {
        public NotFoundException() : base("Not found")
        {
        }

        public NotFoundException(object id) : base($"'${id}' not found")
        {
        }
    }
}
