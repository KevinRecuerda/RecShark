using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RecShark.AspNetCore.Extensions;

namespace RecShark.AspNetCore.Sample.Controllers
{
    [ApiController]
    [ApiVersion("2")]
    [DefaultRoute]
    [SwaggerTagBadgeDefault.Tech]
    public class GenericController : ControllerBase
    {
        /// <summary> generic one </summary>
        [HttpGet("one")]
        public GenericOne<string> GenericOne()
        {
            return new GenericOne<string>() { Item = "one" };
        }

        /// <summary> generic multiple </summary>
        [HttpGet("multiple")]
        public GenericMultiple<string, int> GenericMultiple()
        {
            return new GenericMultiple<string, int>() { Key = "multiple", Value = 2 };
        }

        /// <summary> dico with key sensitive keys </summary>
        [HttpGet("dico")]
        public Dictionary<string, string> Dico()
        {
            return new Dictionary<string, string>()
            {
                ["ENV_DATA"] = "3",
                ["lower"] = "true",
                ["AsOf"] = ""
            };
        }
    }

    public class GenericOne<T>
    {
        public T Item { get; set; }
    }

    public class GenericMultiple<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }
}