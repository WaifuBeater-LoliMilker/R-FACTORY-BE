using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_Factory_BE.Models;
using R_Factory_BE.Services;

namespace R_Factory_BE.Controllers
{
    [Route("device-parameter-logs")]
    [ApiController]
    public class DeviceParameterLogsController : Controller
    {
        private readonly IConfiguration _config;
        private IGenericRepo _repo;
        public DeviceParameterLogsController(IConfiguration config, IGenericRepo repo)
        {
            _config = config;
            _repo = repo;
        }
        [HttpPost("insert-log")]
        [AllowAnonymous]
        [SkipJWTMiddleware]
        public async Task<IActionResult> InsertDataFromWinforms(LogData logData)
        {
            string apiKey = _config["JwtSettings:Secret"]!;
            if (logData.secret != apiKey) return BadRequest("Secret does not match");
            foreach (var value in logData.data)
            {
                if (value.DeviceParameterId <= 0) continue;
                await _repo.Insert<DeviceParameterLogs>(value);
            }
            return Ok();
        }
    }
    public class LogData
    {
        public List<DeviceParameterLogs> data { get; set; } = [];
        public string secret { get; set; } = "";
    }
}
