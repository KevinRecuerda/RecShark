using System;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RecShark.AspNetCore.Options;
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

        public static IServiceCollection AddException(this IServiceCollection services, ExceptionOption option)
        {
            return services.AddProblemDetails(options =>
                           {
                               options.ValidationProblemStatusCode = StatusCodes.Status400BadRequest;

                               options.Map<ArgumentException>((ctx, ex) =>
                               {
                                   var pb = StatusCodeProblemDetails.Create(StatusCodes.Status400BadRequest);
                                   pb.Detail = ex.Message;

                                   var validationPb = new ValidationProblemDetails();
                                   pb.CloneTo(validationPb);
                                   if (ex.ParamName != null)
                                       validationPb.Errors[ex.ParamName] = new[] {ex.Message};

                                   return validationPb;
                               });
                               options.MapToStatusCode<UnauthorizedAccessException>(StatusCodes.Status403Forbidden);
                               options.MapToStatusCode<NotFoundException>(StatusCodes.Status404NotFound);

                               // TODO: param.configure()
                           })
                           .AddProblemDetailsConventions();
        }
    }
}
