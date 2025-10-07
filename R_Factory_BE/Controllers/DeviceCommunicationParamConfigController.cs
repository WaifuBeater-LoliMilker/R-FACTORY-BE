using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_Factory_BE.DTO;
using R_Factory_BE.Models;

namespace R_Factory_BE.Controllers
{
    [Route("device-communication-param-config")]
    [ApiController]
    public class DeviceCommunicationParamConfigController : ControllerBase
    {
        private IGenericRepo _repo;

        public DeviceCommunicationParamConfigController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> GetByDeviceParamId([FromQuery(Name = "device-param-id")] int? deviceParamId)
        {
            if (deviceParamId == null || deviceParamId < 0) deviceParamId = 0;
            var data = await _repo.ProcedureToList<DeviceCommunicationParamConfigDTO>("spGetDeviceCommunicationParamConfig",
                ["DeviceParamId"], [deviceParamId]);
            return Ok(data);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repo.GetById<DeviceCommunicationParamConfig>(id);
            return Ok(data);
        }

        [HttpPost("")]
        [Authorize]
        public async Task<IActionResult> Create(DeviceCommunicationParamConfig commParamConfig)
        {
            await _repo.Insert<DeviceCommunicationParamConfig>(commParamConfig);
            return CreatedAtAction(nameof(GetById), new { commParamConfig.Id }, commParamConfig);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, DeviceCommunicationParamConfig commParamConfig)
        {
            if (id != commParamConfig.Id) return BadRequest("Resouces do not match");
            await _repo.Update<DeviceCommunicationParamConfig>(commParamConfig);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteById<DeviceCommunicationParamConfig>(id);
            return Ok();
        }
    }
}