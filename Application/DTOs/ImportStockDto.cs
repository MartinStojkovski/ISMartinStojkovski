using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class ImportStockDto
    {
        [Required] public string Name { get; set; } = default!;
        [Required] public List<string> Categories { get; set; } = new();
        [Required] public decimal Price { get; set; }
        [Required] public int Quantity { get; set; }
    }
}
