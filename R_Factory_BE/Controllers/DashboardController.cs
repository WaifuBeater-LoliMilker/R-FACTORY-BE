using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace R_Factory_BE.Controllers
{
    [Route("dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        [HttpGet("active-power")]
        [Authorize]
        public IActionResult ActivePowerChart()
        {
            var data = new ChartData
            {
                Data = ["X", "Y", "Z"],
                XAxis = ["A", "B", "C"],
                YAxis = ["a", "b", "c"]
            };
            return Ok();
        }
    }
    public class ChartData
    {
        public string[]? Data { get; set; } = null;
        public string[]? XAxis { get; set; } = null;
        public string[]? YAxis { get; set; } = null;
    }
}
