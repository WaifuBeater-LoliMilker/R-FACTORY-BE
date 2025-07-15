using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_Factory_BE.Models;

namespace R_Factory_BE.Controllers
{
    [Route("communication")]
    [ApiController]
    public class CommunicationController : ControllerBase
    {
        private IGenericRepo _repo;

        public CommunicationController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repo.GetAll<Communication>();
            return Ok(data);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repo.GetById<Communication>(id);
            return Ok(data);
        }

        [HttpPost("")]
        [Authorize]
        public async Task<IActionResult> Create(Communication communication)
        {
            await _repo.Insert<Communication>(communication);
            return CreatedAtAction(nameof(GetById), new { communication.Id }, communication);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, Communication communication)
        {
            if (id != communication.Id) return BadRequest("Resouces do not match");
            await _repo.Update<Communication>(communication);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteById<Communication>(id);
            return Ok();
        }
    }
}