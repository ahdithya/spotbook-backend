using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Request.Space
{
    public class CreateSpaceRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal PricePerHour { get; set; }
    }
}