using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R_Factory_BE.DTO;
using R_Factory_BE.Models;

namespace R_Factory_BE.Controllers
{
    [Route("communication-params")]
    [ApiController]
    public class CommunicationParamsController : ControllerBase
    {
        private IGenericRepo _repo;

        public CommunicationParamsController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> GetParams([FromQuery(Name = "communication-id")] int? commId)
        {
            commId ??= 0;
            var data = await _repo.ProcedureToList<CommunicationParamDTO>("spGetCommunicationParams", ["CommId"], [commId]);
            return Ok(data);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _repo.GetById<CommunicationParam>(id);
            return Ok(data);
        }

        [HttpPost("")]
        [Authorize]
        public async Task<IActionResult> Create(CommunicationParam commParam)
        {
            if (commParam.CommunicationId == 0) return BadRequest("Invalid data");
            await _repo.Insert<CommunicationParam>(commParam);
            return CreatedAtAction(nameof(GetById), new { commParam.Id }, commParam);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, CommunicationParam commParam)
        {
            if (id != commParam.Id) return BadRequest("Resouces do not match");
            if (commParam.CommunicationId == 0) return BadRequest("Invalid data");
            await _repo.Update<CommunicationParam>(commParam);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteById<CommunicationParam>(id);
            return Ok();
        }
    }
}