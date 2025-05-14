using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Moq;
using Xunit;
using Application.Interfaces;

namespace UnitTest
{
    public class StockServiceTests
    {
        private readonly Mock<IRepository<Category>> _catRepo;
        private readonly Mock<IRepository<Product>> _prodRepo;
        private readonly Mock<IRepository<Stock>> _stockRepo;
        private readonly IMapper _mapper;
        private readonly StockService _service;

        public StockServiceTests()
        {
            // We don't need any special maps here, so an empty config is fine:
            _mapper = new MapperConfiguration(cfg => { }).CreateMapper();

            _catRepo = new Mock<IRepository<Category>>();
            _prodRepo = new Mock<IRepository<Product>>();
            _stockRepo = new Mock<IRepository<Stock>>();

            _service = new StockService(
                _catRepo.Object,
                _prodRepo.Object,
                _stockRepo.Object,
                _mapper);
        }

        [Fact]
        public async Task ImportAsync_CreatesMissingCategoryProductAndStock()
        {
            var dto = new ImportStockDto
            {
                Name = "NP",
                Categories = new List<string> { "NewCat" },
                Price = 5.5m,
                Quantity = 3
            };
            _catRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Category>());
            _prodRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Product>());
            _stockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Stock>());

            await _service.ImportAsync(new[] { dto });

            _catRepo.Verify(r => r.AddAsync(
                It.Is<Category>(c => c.Name == "NewCat")), Times.Once);
            _catRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

            _prodRepo.Verify(r => r.AddAsync(
                It.Is<Product>(p => p.Name == "NP" && p.Price == 5.5m)), Times.Once);
            _prodRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

            _stockRepo.Verify(r => r.AddAsync(
                It.Is<Stock>(s => s.Quantity == 3)), Times.Once);
            _stockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ImportAsync_IncrementsExistingStock()
        {
            var category = new Category("C1", null);
            var prod = new Product
            {
                Id = Guid.NewGuid(),
                Name = "NP",
                Price = 2m,
                CategoryId = category.Id,
                Category = category
            };
            var stock = new Stock
            {
                Id = Guid.NewGuid(),
                ProductId = prod.Id,
                Product = prod,
                Quantity = 5,
                LastUpdated = DateTime.UtcNow
            };

            _catRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { category });
            _prodRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { prod });
            _stockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { stock });

            var dto = new ImportStockDto
            {
                Name = "NP",
                Categories = new List<string> { "C1" },
                Price = 2m,
                Quantity = 4
            };

            await _service.ImportAsync(new[] { dto });

            _stockRepo.Verify(r => r.Update(
                It.Is<Stock>(s => s.Quantity == 9)), Times.Once);
            _stockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
