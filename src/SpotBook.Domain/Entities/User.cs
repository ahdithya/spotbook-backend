using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Domain.Enums;

namespace SpotBook.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Customer;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    }
}