using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Application.Mapping;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Moq;
using Xunit;

namespace UnitTest
{
    public class ProductServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IRepository<Product>> _prodRepo;
        private readonly Mock<IRepository<Category>> _catRepo;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new CategoryProfile());
                cfg.AddProfile(new ProductProfile());
            });
            _mapper = config.CreateMapper();

            _prodRepo = new Mock<IRepository<Product>>();
            _catRepo = new Mock<IRepository<Category>>();
            _service = new ProductService(_prodRepo.Object, _catRepo.Object, _mapper);
        }

        [Fact]
        public async Task CreateAsync_WithValidCategory_CreatesAndReturnsDto()
        {
            var category = new Category("CatName", "CatDesc");
            var catId = category.Id;              
            _catRepo.Setup(r => r.GetByIdAsync(catId))
                    .ReturnsAsync(category);

            var createDto = new CreateProductDto
            {
                Name = "TestProd",
                Description = "Desc",
                Price = 9.99m,
                CategoryId = catId
            };

            var result = await _service.CreateAsync(createDto);

            Assert.NotNull(result);
            Assert.Equal("TestProd", result.Name);
            Assert.Equal("Desc", result.Description);
            Assert.Equal(9.99m, result.Price);
            Assert.Equal(catId, result.CategoryId);

            _prodRepo.Verify(r => r.AddAsync(
                It.Is<Product>(p =>
                    p.Name == "TestProd" &&
                    p.Description == "Desc" &&
                    p.Price == 9.99m &&
                    p.CategoryId == catId)),
                Times.Once
            );
            _prodRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithMissingCategory_ThrowsKeyNotFound()
        {
            _catRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((Category?)null);

            var dto = new CreateProductDto
            {
                Name = "X",
                Description = "Y",
                Price = 1m,
                CategoryId = Guid.NewGuid()
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(dto));

            _prodRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
            _prodRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_MapsAllProducts()
        {
            var category = new Category("C1", null);
            var catId = category.Id;
            var products = new[]
            {
                new Product { Id = Guid.NewGuid(), Name="P1", Price=1, CategoryId=catId, Category=category },
                new Product { Id = Guid.NewGuid(), Name="P2", Price=2, CategoryId=catId, Category=category },
            };
            _prodRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
            _catRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { category });

            var result = await _service.GetAllAsync();

            var list = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(result);
            Assert.Collection(list,
                dto => Assert.Equal("P1", dto.Name),
                dto => Assert.Equal("P2", dto.Name)
            );
        }

        [Fact]
        public async Task GetByIdAsync_WhenExists_ReturnsDto()
        {
            var category = new Category("C", null);
            var catId = category.Id;
            var prod = new Product
            {
                Id = Guid.NewGuid(),
                Name = "PX",
                Price = 5,
                CategoryId = catId,
                Category = category
            };
            _prodRepo.Setup(r => r.GetByIdAsync(prod.Id)).ReturnsAsync(prod);
            _catRepo.Setup(r => r.GetByIdAsync(catId)).ReturnsAsync(category);

            var result = await _service.GetByIdAsync(prod.Id);

            Assert.NotNull(result);
            Assert.Equal(prod.Id, result!.Id);
            Assert.Equal("PX", result.Name);
            Assert.Equal(catId, result.CategoryId);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
        {
            _prodRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Product?)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_WhenExists_UpdatesAndReturnsTrue()
        {
            var category = new Category("C", null);
            var catId = category.Id;
            var existing = new Product
            {
                Id = Guid.NewGuid(),
                Name = "O",
                Price = 1m,
                CategoryId = catId,
                Category = category
            };
            _prodRepo.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
            _catRepo.Setup(r => r.GetByIdAsync(catId)).ReturnsAsync(category);

            var updateDto = new UpdateProductDto
            {
                Name = "N",
                Description = "D2",
                Price = 2.22m,
                CategoryId = catId
            };

            var result = await _service.UpdateAsync(existing.Id, updateDto);

            Assert.True(result);
            Assert.Equal("N", existing.Name);        
            Assert.Equal("D2", existing.Description);
            _prodRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotExists_ReturnsFalse()
        {
            _prodRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Product?)null);

            var result = await _service.UpdateAsync(Guid.NewGuid(), new UpdateProductDto());

            Assert.False(result);
            _prodRepo.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
            _prodRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenExists_DeletesAndReturnsTrue()
        {
            var existing = new Product { Id = Guid.NewGuid(), Name = "X" };
            _prodRepo.Setup(r => r.GetByIdAsync(existing.Id))
                     .ReturnsAsync(existing);

            var result = await _service.DeleteAsync(existing.Id);

            Assert.True(result);
            _prodRepo.Verify(r => r.Remove(existing), Times.Once);
            _prodRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotExists_ReturnsFalse()
        {
            _prodRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Product?)null);

            var result = await _service.DeleteAsync(Guid.NewGuid());

            Assert.False(result);
            _prodRepo.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
            _prodRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
