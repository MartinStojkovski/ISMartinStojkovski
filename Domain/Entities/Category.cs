using System;

namespace Domain.Entities
{
    public class Category
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public Category(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));

            Id = Guid.NewGuid();
            Name = name;
            Description = description;
        }
    }
}
