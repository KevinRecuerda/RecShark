using System;
using Microsoft.AspNetCore.Mvc;
using RecShark.AspNetCore.Extensions.Extensions;
using RecShark.AspNetCore.Extensions.Options;

namespace RecShark.AspNetCore.Sample.Controllers
{
    [ApiController]
    [ApiVersion("2")]
    [DefaultRoute]
    [SwaggerTagBadgeDefault.Tech]
    public class ExceptionController : ControllerBase
    {
        /// <summary> ArgumentException => 400 bad request </summary>
        [HttpGet("argument")]
        public void Argument()
        {
            throw new ArgumentException("arg");
        }

        /// <summary> UnauthorizedAccessException => 403 forbidden  </summary>
        [HttpGet("forbidden")]
        public void Forbidden()
        {
            throw new UnauthorizedAccessException();
        }

        /// <summary> NotFoundException => 404 not found  </summary>
        [HttpGet("notfound")]
        public void NotFound()
        {
            throw new NotFoundException();
        }

        /// <summary> Internal exception => 500 internal error  </summary>
        [HttpGet("internal")]
        public void Internal()
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