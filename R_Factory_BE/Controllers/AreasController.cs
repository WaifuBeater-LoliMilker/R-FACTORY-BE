using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_Factory_BE.Models;

namespace R_Factory_BE.Controllers
{
    [Route("areas")]
    [ApiController]
    public class AreasController : ControllerBase
    {
        private IGenericRepo _repo;

        public AreasController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repo.GetAll<Areas>();
            return Ok(data);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repo.GetById<Areas>(id);
            return Ok(data);
        }

        [HttpPost("")]
        [Authorize]
        public async Task<IActionResult> Create(Areas area)
        {
            await _repo.Insert<Areas>(area);
            return CreatedAtAction(nameof(GetById), new { area.Id }, area);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, Areas area)
        {
            if (id != area.Id) return BadRequest("Resouces do not match");
            await _repo.Update<Areas>(area);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteById<Areas>(id);
            return Ok();
        }
    }
}