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
            var data = await _repo.ProcedureToList<ElectricUsageChartData, ElectricUsageChartData>(
                "spGetEnergyConsumptionDailySums", [], []);
            return Ok(data);
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
        [HttpGet("details")]
        [Authorize]
        public async Task<IActionResult> Details([FromQuery(Name = "date-option")] int dateOption)
        {
            var voltageData = await _repo.ProcedureToList<DetailCharts>(
                    "spGetDetailChartData", ["ChartType", "DateOption"], [1, dateOption]);
            var amperageData = await _repo.ProcedureToList<DetailCharts>(
                    "spGetDetailChartData", ["ChartType", "DateOption"], [2, dateOption]);
            var powerRateData = await _repo.ProcedureToList<DetailCharts>(
                    "spGetDetailChartData", ["ChartType", "DateOption"], [3, dateOption]);
            var temperatureData = await _repo.ProcedureToList<DetailCharts>(
                    "spGetDetailChartData", ["ChartType", "DateOption"], [4, dateOption]);
            var wasteOutputData = await _repo.ProcedureToList<DetailCharts>(
                    "spGetDetailChartData", ["ChartType", "DateOption"], [5, dateOption]);
            try
            {
                var result = new
                {
                    voltageData,
                    amperageData,
                    powerRateData,
                    temperatureData,
                    wasteOutputData,
                };
                return Ok(result);
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