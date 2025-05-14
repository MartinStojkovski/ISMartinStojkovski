using System.Collections.Generic;

namespace Application.DTOs
{
    public class DiscountResultDto
    {
        public List<DiscountLineDto> Lines { get; set; } = new();
        public decimal TotalBeforeDiscount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalAfterDiscount { get; set; }
    }
}
