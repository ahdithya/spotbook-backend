using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Response.Voucher
{
    public class VoucherDiscountResult
    {
        public Guid VoucherId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal OriginalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}