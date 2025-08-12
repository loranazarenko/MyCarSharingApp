using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarSharingApp.Application.DTOs;
using MyCarSharingApp.Application.Interfaces;
using MyCarSharingApp.Application.Services;
using System.Data;

namespace MyCarSharingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly ICarService _service;
        private readonly ILogger<CarController> _logger;
        public CarController(ICarService service, ILogger<CarController> logger)
        {
            _service = service;
            _logger = logger;
        } 

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CarResponseDto>> Add([FromBody] CarRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var created = await _service.AddNewCarAsync(dto);
            _logger.LogInformation("Created new car");
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet, AllowAnonymous]
        public async Task<IEnumerable<CarResponseDto>> List([FromQuery] int page = 1, [FromQuery] int size = 10)
            => await _service.GetAllCarsAsync(page, size);

        [HttpGet("{id}"), AllowAnonymous]
        public async Task<ActionResult<CarResponseDto>> GetById(int id)
            => Ok(await _service.GetCarByIdAsync(id));

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CarResponseDto>> Update(int id, [FromBody] CarRequestDto dto) { 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(await _service.UpdateCarByIdAsync(id, dto));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteCarByIdAsync(id);
            return NoContent();
        }
    }
}