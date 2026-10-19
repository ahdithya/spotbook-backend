using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Domain.Enums;

namespace SpotBook.Domain.Entities
{
    public class Voucher
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = string.Empty; // Misal: "HEMAT20"
        public DiscountType DiscountType { get; set; } = DiscountType.Percentage; // "Percentage" atau "FixedAmount"
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int Quota { get; set; }
        public int UsedCount { get; set; } = 0;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool IsActive { get; set; } = true;
    }
}