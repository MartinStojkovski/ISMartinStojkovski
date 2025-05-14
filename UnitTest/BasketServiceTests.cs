using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Moq;
using Xunit;

namespace UnitTest
{
    public class BasketServiceTests
    {
        private readonly Mock<IRepository<Product>> _prodRepo = new();
        private readonly Mock<IRepository<Stock>> _stockRepo = new();
        private readonly BasketService _service;

        public BasketServiceTests()
        {
            _service = new BasketService(
                _prodRepo.Object,
                _stockRepo.Object);
        }

        [Fact]
        public async Task CalculateDiscountAsync_SingleItem_NoDiscount()
        {
            var prodId = Guid.NewGuid();
            var prod = new Product { Id = prodId, Name = "P", Price = 10m };
            var stock = new Stock { ProductId = prodId, Quantity = 5 };

            _prodRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { prod });
            _stockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { stock });

            var basket = new[]
            {
                new BasketItemDto { ProductId = prodId, Quantity = 1 }
            };

            var result = await _service.CalculateDiscountAsync(basket);

            Assert.Equal(0m, result.TotalDiscount);
            Assert.Single(result.Lines);
            Assert.Equal(10m, result.Lines.Single().LineTotal);
        }

        [Fact]
        public async Task CalculateDiscountAsync_MultipleSame_5PercentOnFirst()
        {
            var prodId = Guid.NewGuid();
            var prod = new Product { Id = prodId, Name = "P", Price = 100m };
            var stock = new Stock { ProductId = prodId, Quantity = 5 };

            _prodRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { prod });
            _stockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { stock });

            var basket = new[]
            {
                new BasketItemDto { ProductId = prodId, Quantity = 3 }
            };

            var result = await _service.CalculateDiscountAsync(basket);

            Assert.Equal(100m * 0.05m, result.TotalDiscount);
            Assert.Single(result.Lines);
            Assert.Equal(100m * 3 - 5m, result.Lines.Single().LineTotal);
        }

        [Fact]
        public async Task CalculateDiscountAsync_InsufficientStock_Throws()
        {
            var prodId = Guid.NewGuid();
            var prod = new Product { Id = prodId, Name = "P", Price = 1m };
            var stock = new Stock { ProductId = prodId, Quantity = 1 };

            _prodRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { prod });
            _stockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { stock });

            var basket = new[]
            {
                new BasketItemDto { ProductId = prodId, Quantity = 2 }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CalculateDiscountAsync(basket)
            );
        }
    }
}
