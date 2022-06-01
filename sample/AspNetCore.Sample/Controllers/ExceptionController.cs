using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecShark.AspNetCore.Configurator;
using RecShark.AspNetCore.Extensions;

namespace RecShark.AspNetCore.Sample.Controllers
{
    [ApiController]
    [ApiVersion("2")]
    [DefaultRoute]
    [SwaggerTagBadgeDefault.Tech]
    public class ExceptionController : ControllerBase
    {
        /// <summary> Validation object => 400 bad request </summary>
        [HttpPost("validation")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public void ValidationCode(Filter filter)
        {
        }

        /// <summary> ArgumentException => 400 bad request </summary>
        [HttpGet("badrequest")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public void BadRequestCode()
        {
            throw new ArgumentException("arg");
        }

        /// <summary> UnauthorizedAccessException => 403 forbidden  </summary>
        [HttpGet("forbidden")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public void ForbiddenCode()
        {
            throw new UnauthorizedAccessException();
        }

        /// <summary> NotFoundException => 404 not found  </summary>
        [HttpGet("notfound")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public void NotFoundCode()
        {
            throw new NotFoundException();
        }

        /// <summary> Internal exception => 500 internal error  </summary>
        [HttpGet("internal")]
        public void InternalCode()
        {
            throw new InternalException();
        }

        /// <summary> AggregateException => use inner </summary>
        [HttpGet("aggregate")]
        public void Aggregate()
        {
            var innerException = new ArgumentException("arg");
            throw new AggregateException(innerException);
        }
    }

    public class InternalException : Exception
    {
    }

    public class Filter : IValidatableObject
    {
        public string[]  Ids  { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To   { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Ids.Any())
                yield return new ValidationResult("'ids' must not be empty", new[] {nameof(Ids)});

            if (To < From)
                yield return new ValidationResult("'from' must be before 'to'", new[] {nameof(From), nameof(To)});
        }
    }
}
