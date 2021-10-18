using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RecShark.AspNetCore.Extensions;

namespace RecShark.AspNetCore.Sample.Controllers
{
    [ApiController]
    [ApiVersion("2")]
    [DefaultRoute]
    [SwaggerTagBadgeDefault.Tech]
    public class EnumController : ControllerBase
    {
        /// <summary> Within object </summary>
        [HttpGet("within-object")]
        public EnumItem WithinObject()
        {
            return new EnumItem()
            {
                Name = "within object",
                Frequency = Frequency.Daily,
                Frequencies = new[] { Frequency.Daily, Frequency.Weekly, Frequency.Monthly },
                DicoFrequency = new Dictionary<Frequency, Frequency>()
                {
                    [Frequency.Daily] = Frequency.Daily,
                    [Frequency.Weekly] = Frequency.Weekly,
                    [Frequency.Monthly] = Frequency.Monthly
                },
                DicoFrequencies = new Dictionary<Frequency, Frequency[]>()
                {
                    [Frequency.Daily] = new[] { Frequency.Daily, Frequency.Weekly }
                }
            };
        }
        
        /// <summary> As parameter </summary>
        [HttpGet("parameter")]
        public Frequency Parameter(Frequency frequency)
        {
            return frequency;
        }
        
        /// <summary> As nullable parameter </summary>
        [HttpGet("parameter-nullable")]
        public Frequency? ParameterNullable(Frequency? frequency)
        {
            return frequency;
        }
        
        /// <summary> As multiple parameter </summary>
        [HttpGet("parameter-multiple")]
        public Frequency[] ParameterMultiple([FromQuery] Frequency[] frequencies)
        {
            return frequencies;
        }
        
        /// <summary> As nullable multiple parameter </summary>
        [HttpGet("parameter-multiple-nullable")]
        public Frequency?[] ParameterMultiple([FromQuery] Frequency?[] frequencies)
        {
            return frequencies;
        }
        
        /// <summary> From unused enum </summary>
        [HttpGet("unused")]
        public Dictionary<UnusedEnum, UnusedEnum> Unused()
        {
            return new Dictionary<UnusedEnum, UnusedEnum>()
            {
                [UnusedEnum.Ok] = UnusedEnum.Error
            };
        }

        public enum UnusedEnum
        {
            Ok = 1,
            Error = 2
        }
    }

    public enum Frequency
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }

    public class EnumItem
    {
        public string Name { get; set; }
        public Frequency Frequency { get; set; }
        public Frequency[] Frequencies { get; set; }
        public Dictionary<Frequency, Frequency> DicoFrequency { get; set; }
        public Dictionary<Frequency, Frequency[]> DicoFrequencies { get; set; }
    }
}