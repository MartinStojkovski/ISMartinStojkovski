using System;

namespace Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }               // primary key
        public string Name { get; set; }           // required
        public string? Description { get; set; }   // optional
        public decimal Price { get; set; }         // required
        public Guid CategoryId { get; set; }       // FK to Category
        public Category Category { get; set; }     // navigation
    }
}
