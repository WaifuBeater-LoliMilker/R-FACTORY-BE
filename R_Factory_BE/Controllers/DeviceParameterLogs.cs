using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using R_Factory_BE.Middlewares;
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
            try
            {
                string apiKey = _config["JwtSettings:Secret"]!;
                if (logData.secret != apiKey) return BadRequest("Secret does not match");
                foreach (var value in logData.data)
                {
                    if (value.DeviceParameterId <= 0) continue;
                    await _repo.Insert<DeviceParameterLogs>(value);

                    await _repo.ExecuteProcedureAsync(
                        "spAggregateEnergyHourly_ByLog",
                        new string[] { "@pDeviceParameterId", "@pLogTime"},
                        new object[] { value.DeviceParameterId, value.LogTime }
                    );

                    await _repo.ExecuteProcedureAsync(
                        "spAggregateEnergyDaily_ByLog",
                        new string[] { "@pDeviceParameterId", "@pLogTime"},
                        new object[] { value.DeviceParameterId, value.LogTime }
                    );

                    await _repo.ExecuteProcedureAsync(
                        "spAggregateEnergyMonthly_ByLog",
                        new string[] { "@pDeviceParameterId", "@pYear", "@pMonth" },
                        new object[] { value.DeviceParameterId, value.LogTime.Year, value.LogTime.Month }
                    );
                }
                return Ok();
            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ex);
                return BadRequest();
            }
        }
    }

    public class LogData
    {
        public List<DeviceParameterLogs> data { get; set; } = [];
        public string secret { get; set; } = "";
    }
}