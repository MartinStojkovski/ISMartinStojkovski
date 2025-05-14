using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class BasketItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
