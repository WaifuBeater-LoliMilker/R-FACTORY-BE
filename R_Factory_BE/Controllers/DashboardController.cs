using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_Factory_BE.DTO;

namespace R_Factory_BE.Controllers
{
    [Route("dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private IGenericRepo _repo;
        public DashboardController(IGenericRepo repo)
        {
            _repo = repo;
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
    }
}
