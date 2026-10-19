using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Response.Voucher
{
    public class VoucherResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int Quota { get; set; }
        public int UsedCount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool IsActive { get; set; }
    }
}