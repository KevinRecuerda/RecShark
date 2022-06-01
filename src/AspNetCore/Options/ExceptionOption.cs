using System;
using System.Collections.Generic;
using System.Net;

namespace RecShark.AspNetCore.Options
{
    public class ExceptionOption
    {
        public bool SkipAggregateException { get; set; }

        public Dictionary<Type, HttpStatusCode> ExceptionStatusCodes { get; set; } = new Dictionary<Type, HttpStatusCode>
        {
            [typeof(ArgumentException)]           = HttpStatusCode.BadRequest,
            [typeof(UnauthorizedAccessException)] = HttpStatusCode.Forbidden,
            [typeof(NotFoundException)]           = HttpStatusCode.NotFound
        };
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
