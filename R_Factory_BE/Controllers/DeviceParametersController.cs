using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_Factory_BE.DTO;
using R_Factory_BE.Models;

namespace R_Factory_BE.Controllers
{
    [Route("device-parameters")]
    [ApiController]
    public class DeviceParametersController : ControllerBase
    {
        private IGenericRepo _repo;

        public DeviceParametersController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> GetByDeviceId([FromQuery(Name = "device-id")] int? deviceId)
        {
            deviceId ??= 0;
            var data = await _repo.ProcedureToList<DeviceParamDTO>("spGetDeviceParam", ["DeviceId"], [deviceId]);
            return Ok(data);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repo.GetById<DeviceParameters>(id);
            return Ok(data);
        }

        [HttpPost("")]
        [Authorize]
        public async Task<IActionResult> Create(DeviceParamDTO deviceParam)
        {
            await _repo.Insert<DeviceParameters>(deviceParam);
            if(deviceParam.ConfigValues != null)
                foreach (var configValue in deviceParam.ConfigValues)
                {
                    await _repo.Insert<DeviceCommunicationParamConfig>(configValue);
                }
            return CreatedAtAction(nameof(GetById), new { deviceParam.Id }, deviceParam);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, DeviceParamDTO deviceParam)
        {
            if (id != deviceParam.Id) return BadRequest("Resouces do not match");
            await _repo.Update<DeviceParameters>(deviceParam);
            if (deviceParam.ConfigValues != null)
                foreach (var configValue in deviceParam.ConfigValues)
                {
                    if (configValue.Id > 0)
                        await _repo.Update<DeviceCommunicationParamConfig>(configValue);
                    else
                        await _repo.Insert<DeviceCommunicationParamConfig>(configValue);
                }
            return Ok(deviceParam);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteById<DeviceParameters>(id);
            return Ok();
        }
    }
}