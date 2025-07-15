using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_Factory_BE.Models;

namespace R_Factory_BE.Controllers
{
    [Route("communication-param-config")]
    [ApiController]
    public class CommunicationParamConfigController : ControllerBase
    {
        private IGenericRepo _repo;

        public CommunicationParamConfigController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repo.GetAll<CommunicationParamConfig>();
            return Ok(data);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repo.GetById<CommunicationParamConfig>(id);
            return Ok(data);
        }

        [HttpPost("")]
        [Authorize]
        public async Task<IActionResult> Create(CommunicationParamConfig commParamConfig)
        {
            await _repo.Insert<CommunicationParamConfig>(commParamConfig);
            return CreatedAtAction(nameof(GetById), new { commParamConfig.Id }, commParamConfig);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, CommunicationParamConfig commParamConfig)
        {
            if (id != commParamConfig.Id) return BadRequest("Resouces do not match");
            await _repo.Update<CommunicationParamConfig>(commParamConfig);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteById<CommunicationParamConfig>(id);
            return Ok();
        }
    }
}