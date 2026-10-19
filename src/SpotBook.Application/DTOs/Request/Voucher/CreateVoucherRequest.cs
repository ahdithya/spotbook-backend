using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Domain.Enums;

namespace SpotBook.Application.DTOs.Request.Voucher
{
    public class CreateVoucherRequest
    {
        public string Code { get; set; } = string.Empty;
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int Quota { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}