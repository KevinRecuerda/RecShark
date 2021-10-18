using System;
using Microsoft.AspNetCore.Mvc;
using RecShark.AspNetCore.Extensions;
using RecShark.AspNetCore.Options;

namespace RecShark.AspNetCore.Sample.Controllers
{
    [ApiController]
    [ApiVersion("2")]
    [DefaultRoute]
    [SwaggerTagBadgeDefault.Tech]
    public class ExceptionController : ControllerBase
    {
        /// <summary> ArgumentException => 400 bad request </summary>
        [HttpGet("badrequest")]
        public void BadRequestCode()
        {
            throw new ArgumentException("arg");
        }

        /// <summary> UnauthorizedAccessException => 403 forbidden  </summary>
        [HttpGet("forbidden")]
        public void ForbiddenCode()
        {
            throw new UnauthorizedAccessException();
        }

        /// <summary> NotFoundException => 404 not found  </summary>
        [HttpGet("notfound")]
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
}