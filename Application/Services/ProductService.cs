using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _products;
        private readonly IRepository<Category> _categories;
        private readonly IMapper _mapper;

        public ProductService(
            IRepository<Product> products,
            IRepository<Category> categories,
            IMapper mapper)
        {
            _products = products;
            _categories = categories;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _products.GetAllAsync();
            var categories = await _categories.GetAllAsync();

            foreach (var p in products)
            {
                p.Category = categories
                    .SingleOrDefault(c => c.Id == p.CategoryId)
                    ?? throw new KeyNotFoundException(
                        $"Category {p.CategoryId} not found for product {p.Id}.");
            }

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var product = await _products.GetByIdAsync(id);
            if (product == null)
                return null;

            product.Category = await _categories.GetByIdAsync(product.CategoryId)
                ?? throw new KeyNotFoundException(
                    $"Category {product.CategoryId} not found for product {id}.");

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var category = await _categories.GetByIdAsync(dto.CategoryId)
                ?? throw new KeyNotFoundException($"Category {dto.CategoryId} not found.");

            var product = _mapper.Map<Product>(dto);
            product.Id = Guid.NewGuid();

            await _products.AddAsync(product);
            await _products.SaveChangesAsync();

            product.Category = category;

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateProductDto dto)
        {
            var product = await _products.GetByIdAsync(id);
            if (product == null)
                return false;

            _ = await _categories.GetByIdAsync(dto.CategoryId)
                ?? throw new KeyNotFoundException($"Category {dto.CategoryId} not found.");

            _mapper.Map(dto, product);
            await _products.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var product = await _products.GetByIdAsync(id);
            if (product == null)
                return false;

            _products.Remove(product);
            await _products.SaveChangesAsync();
            return true;
        }
    }
}
