using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Application.DTOs.Request.Booking;
using SpotBook.Application.DTOs.Response.Booking;

namespace SpotBook.Application.Interfaces.Bookings
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request);
        Task<IEnumerable<BookingResponse>> GetUserBookingsAsync(Guid userId);
        Task<BookingResponse?> GetBookingByIdAsync(Guid bookingId, Guid userId);
        Task<bool> CancelBookingAsync(Guid bookingId, Guid userId);
    }
}