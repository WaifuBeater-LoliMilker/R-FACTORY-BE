using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> GetAll()
        {
            var data = await _repo.GetAll<DeviceParameters>();
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
        public async Task<IActionResult> Create(DeviceParameters deviceParam)
        {
            await _repo.Insert<DeviceParameters>(deviceParam);
            return CreatedAtAction(nameof(GetById), new { deviceParam.Id }, deviceParam);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, DeviceParameters deviceParam)
        {
            if (id != deviceParam.Id) return BadRequest("Resouces do not match");
            await _repo.Update<DeviceParameters>(deviceParam);
            return Ok();
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