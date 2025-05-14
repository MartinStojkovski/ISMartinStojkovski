using System;

namespace Application.DTOs
{
    public class DiscountLineDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal LineTotal { get; set; }
    }
}
