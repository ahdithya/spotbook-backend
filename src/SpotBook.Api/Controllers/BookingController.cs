using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBook.Application.DTOs.Request.Booking;
using SpotBook.Application.DTOs.Response.Booking;
using SpotBook.Application.Interfaces.Bookings;

namespace SpotBook.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController(IBookingService bookingService) : ControllerBase
    {
        private readonly IBookingService _bookingService = bookingService;

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Identitas pengguna tidak valid.");
            }

            return userId;
        }


        [HttpPost]
        public async Task<ActionResult<BookingResponse>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _bookingService.CreateBookingAsync(userId, request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-bookings")]
        public async Task<ActionResult<IEnumerable<BookingResponse>>> GetMyBookings()
        {
            var userId = GetCurrentUserId();
            var result = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(result);
        }


        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BookingResponse>> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _bookingService.GetBookingByIdAsync(id, userId);
            if (result == null)
            {
                return NotFound(new { message = "Booking tidak ditemukan." });
            }
            return Ok(result);
        }


        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _bookingService.CancelBookingAsync(id, userId);
                if (!success)
                {
                    return NotFound(new { message = "Booking tidak ditemukan." });
                }
                return Ok(new { message = "Pemesanan berhasil dibatalkan." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}