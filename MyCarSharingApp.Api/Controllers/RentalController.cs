using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarSharingApp.Application.DTOs;
using MyCarSharingApp.Application.Interfaces;
using MyCarSharingApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCarSharingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalController : ControllerBase
    {
        private readonly IRentalService _rentalService;
        private readonly ILogger<RentalController> _logger;

        public RentalController(IRentalService rentalService, ILogger<RentalController> logger)
        {
            _rentalService = rentalService;
            _logger = logger;
        }

        // POST /api/rental
        [HttpPost]
        [Authorize(Roles = "Admin")]               
        [ProducesResponseType(typeof(RentalResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddRental([FromBody] RentalRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _rentalService.RentCarAsync(requestDto);
            _logger.LogInformation("New rental created");
            // return 201 Created with Location tittle (GetById)
            return CreatedAtAction(nameof(GetRentalById),
                new { rentalId = created.RentalId },
                created);
        }

        // GET /api/rental?userId=...&isActive=true
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<RentalResponseDto>), 200)]
        public async Task<IActionResult> GetRentalsByUserId(
            [FromQuery] string? userId = null,
            [FromQuery] bool? isActive = null)
        {
            var list = await _rentalService.GetRentalsByUserIdAsync(userId, isActive);
            return Ok(list);
        }

        [HttpGet("details")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetWithDetails([FromQuery] string? userId = null, [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var items = await _rentalService.GetRentalsWithDetailsAsync(userId, isActive, page, pageSize);
            return Ok(items);
        }

        // GET /api/rental/{rentalId}
        [HttpGet("{rentalId:int}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(typeof(RentalResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRentalById([FromRoute] int rentalId)
        {
            var rental = await _rentalService.GetRentalByIdAsync(rentalId);
            if (rental == null) return NotFound();
            return Ok(rental);
        }

        // PUT /api/rental/{rentalId}/return
        [HttpPut("{rentalId:int}/return")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RentalResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SetActualReturnDate([FromRoute] int rentalId)
        {
            var updated = await _rentalService.SetActualReturnDateAsync(rentalId);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
    }
}
