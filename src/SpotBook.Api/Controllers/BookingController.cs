using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBook.Application.Common.Exceptions;
using SpotBook.Application.Common.Models;
using SpotBook.Application.DTOs.Request.Booking;
using SpotBook.Application.DTOs.Response.Booking;
using SpotBook.Application.Interfaces.Bookings;

namespace SpotBook.Api.Controllers;

[Authorize]
public class BookingController(IBookingService bookingService) : BaseApiController
{
    private readonly IBookingService _bookingService = bookingService;

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var result = await _bookingService.CreateBookingAsync(CurrentUserId, request);
        return HandleCreated(nameof(GetById), new { id = result.Id }, result, "Pemesanan berhasil dibuat.");
    }

    [HttpGet("my-bookings")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BookingResponse>>>> GetMyBookings()
    {
        var result = await _bookingService.GetUserBookingsAsync(CurrentUserId);
        return HandleSuccess(result, "Riwayat pemesanan berhasil diambil.");
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> GetById(Guid id)
    {
        var result = await _bookingService.GetBookingByIdAsync(id, CurrentUserId)
            ?? throw new NotFoundException("Booking tidak ditemukan.");
        return HandleSuccess(result, "Detail pemesanan berhasil diambil.");
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<object>>> CancelBooking(Guid id)
    {
        var success = await _bookingService.CancelBookingAsync(id, CurrentUserId);
        if (!success)
        {
            throw new NotFoundException("Booking tidak ditemukan.");
        }
        return HandleSuccess("Pemesanan berhasil dibatalkan.");
    }
}
