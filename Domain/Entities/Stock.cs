using System;

namespace Domain.Entities
{
    public class Stock
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = default!;
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
