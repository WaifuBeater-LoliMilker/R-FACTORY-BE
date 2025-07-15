using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_Factory_BE.DTO;
using R_Factory_BE.Models;

namespace R_Factory_BE.Controllers
{
    [Route("devices")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private IGenericRepo _repo;

        public DevicesController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> GetDevices([FromQuery(Name = "area-id")] int? areaId)
        {
            areaId ??= 0;
            var data = await _repo.ProcedureToList<DevicesDTO>("spGetDevices", ["AreaId"], [areaId]);
            return Ok(data);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repo.GetById<Devices>(id);
            return Ok(data);
        }

        [HttpPost("")]
        [Authorize]
        public async Task<IActionResult> Create(Devices device)
        {
            await _repo.Insert<Devices>(device);
            return CreatedAtAction(nameof(GetById), new { device.Id }, device);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, Devices device)
        {
            if (id != device.Id) return BadRequest("Resouces do not match");
            await _repo.Update<Devices>(device);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteById<Devices>(id);
            return Ok();
        }
    }
}