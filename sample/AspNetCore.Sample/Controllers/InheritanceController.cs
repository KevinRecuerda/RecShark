using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NJsonSchema.Converters;
using RecShark.AspNetCore.Extensions.Extensions;

namespace RecShark.AspNetCore.Sample.Controllers
{
    [ApiController]
    [ApiVersion("2")]
    [DefaultRoute]
    [SwaggerTagBadgeDefault.Tech]
    public class InheritanceController : ControllerBase
    {
        /// <summary> concrete base class </summary>
        [HttpGet("concrete")]
        public InheritanceItem[] Concrete()
        {
            return new[]
            {
                new InheritanceItem() { Name = "item" },
                new InheritanceItemDetail() { Name = "item detail", Description = "some description" }
            };
        }

        /// <summary> abstract base class </summary>
        [HttpGet("abstract")]
        public InheritanceAnimal[] Abstract()
        {
            return new InheritanceAnimal[]
            {
                new InheritanceLion() { Name = "lion", Strength = 100 },
                new InheritanceFish() { Name = "fish", Size = 1 }
            };
        }
    }

    [JsonConverter(typeof(JsonInheritanceConverter))]
    [KnownType(typeof(InheritanceItemDetail))]
    public class InheritanceItem
    {
        public string Name { get; set; }
    }

    public class InheritanceItemDetail : InheritanceItem
    {
        public string Description { get; set; }
    }

    [JsonConverter(typeof(JsonInheritanceConverter))]
    [KnownType(typeof(InheritanceLion))]
    [KnownType(typeof(InheritanceFish))]
    public abstract class InheritanceAnimal
    {
        public string Name { get; set; }
    }

    public class InheritanceLion : InheritanceAnimal
    {
        public int Strength { get; set; }
    }

    public class InheritanceFish : InheritanceAnimal
    {
        public int Size { get; set; }
    }
}