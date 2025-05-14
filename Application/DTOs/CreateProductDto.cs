using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateProductDto
    {
        [Required] public string Name { get; set; } = default!;
        public string? Description { get; set; }
        [Required][Range(0.01, double.MaxValue)] public decimal Price { get; set; }
        [Required] public Guid CategoryId { get; set; }
    }
}
