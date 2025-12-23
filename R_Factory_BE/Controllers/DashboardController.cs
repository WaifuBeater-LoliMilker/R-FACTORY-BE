using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_Factory_BE.DTO;
using R_Factory_BE.Models;
using R_Factory_BE.Services;

namespace R_Factory_BE.Controllers
{
    [Route("dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IGenericRepo _repo;

        public DashboardController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("end-points")]
        [AllowAnonymous]
        [SkipJWTMiddleware]
        public async Task<IActionResult> GetEndPoints()
        {
            var data = await _repo.ProcedureToList<Endpoints, ModbusDetails, StartAddresses>(
                "spGetEnpoints", [], []);
            return Ok(data);
        }

        [HttpGet("latest-change")]
        [AllowAnonymous]
        [SkipJWTMiddleware]
        public async Task<IActionResult> LatestChange()
        {
            var data = await _repo.FindModel<AuditLog>(l => l.TableName == "device_communication_param_config");
            return Ok(data?.LastModified ?? DateTime.MinValue);
        }

        [HttpGet("org-chart")]
        [Authorize]
        public async Task<IActionResult> OrgChartData()
        {
            var data = await _repo.ProcedureToList<OrgChartData>("spGetOrgChartData", [], []);
            return Ok(data);
        }

        [HttpGet("active-power-chart")]
        [Authorize]
        public async Task<IActionResult> ActivePowerData()
        {
            var data = await _repo.ProcedureToList<ActivePowerChartData>("spGetActivePowerChartData", [], []);
            return Ok(data);
        }

        [HttpGet("energy-consumption-chart")]
        [Authorize]
        public async Task<IActionResult> EnergyConsumptionData()
        {
            var data = await _repo.ProcedureToList<EnergyConsumptionChartData>("spGetEnergyConsumptionChartData", [], []);
            return Ok(data);
        }

        [HttpGet("electric-usage-chart")]
        [Authorize]
        public async Task<IActionResult> ElectricUsageData()
        {
            var data = await _repo.ProcedureToList<ElectricUsageChartData>(
                "spGetEnergyConsumptionDailySums", [], []);
            var now = DateTime.Today;
            var curYear = now.Year;
            var curMonth = now.Month;

            var prev = now.AddMonths(-1);
            var prevYear = prev.Year;
            var prevMonth = prev.Month;

            var result = new
            {
                Item1 = data
                    .Where(x => x.YearValue == curYear && x.MonthValue == curMonth)
                    .OrderBy(x => x.DayDate)
                    .ToList(),

                Item2 = data
                    .Where(x => x.YearValue == prevYear && x.MonthValue == prevMonth)
                    .OrderBy(x => x.DayDate)
                    .ToList()
            };

            return Ok(result);
        }

        [HttpGet("waste-output-chart")]
        [Authorize]
        public async Task<IActionResult> WasteOutputData()
        {
            var data = await _repo.ProcedureToList<WasteOutputChartData>("spGetWasteOutputChartData", [], []);
            return Ok(data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateOption"></param>
        /// <returns></returns>
        [HttpGet("details-energy")]
        [Authorize]
        public async Task<IActionResult> DetailsEnergy([FromQuery(Name = "year")] int year,
            [FromQuery(Name = "month")] int month,
            [FromQuery(Name = "device-id")] int deviceId)
        {
            try
            {
                var powerRateData = await _repo.ProcedureToList<DetailCharts>(
                        "spGetDetailEnergyChartData", ["YearValue", "MonthValue", "DeviceId"], [year, month, deviceId]);
                return Ok(powerRateData);
            }
            catch
            {
                return StatusCode(500, "Load dữ liệu thất bại");
            }
        }
        [HttpGet("details-waste-output")]
        [Authorize]
        public async Task<IActionResult> DetailsWasteOutput([FromQuery(Name = "year")] int year,
            [FromQuery(Name = "month")] int month,
            [FromQuery(Name = "device-id")] int deviceId)
        {
            try
            {
                var wasteOutputData = await _repo.ProcedureToList<DetailCharts>(
                        "spGetDetailWasteOutputChartData", ["YearValue", "MonthValue", "DeviceId"], [year, month, deviceId]);
                return Ok(wasteOutputData);
            }
            catch
            {
                return StatusCode(500, "Load dữ liệu thất bại");
            }
        }
    }

    public record Endpoints(string IP = "", string Port = "");
    public record ModbusDetails(string SlaveId = "", string FunctionRead = "");
    public record StartAddresses(int DeviceParameterId = 0, string StartAddress = "");
}