using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Request.Voucher
{
    public class ApplyVoucherRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
    }
}