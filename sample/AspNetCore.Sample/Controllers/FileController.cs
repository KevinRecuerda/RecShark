using Microsoft.AspNetCore.Mvc;
using RecShark.AspNetCore.Extensions.Extensions;

namespace RecShark.AspNetCore.Sample.Controllers
{
    [ApiController]
    [ApiVersion("2")]
    [DefaultRoute]
    [SwaggerTagBadgeDefault.Data]
    public class FileController : ControllerBase
    {
        /// <summary> excel file </summary>
        [HttpGet("excel")]
        public FileContentResult Excel()
        {
            var bytes = System.IO.File.ReadAllBytes("data/sample.xlsx");
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "sample");
        }
    }
}