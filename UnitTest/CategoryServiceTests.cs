using System;
using System.Collections.Generic;
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
    public class CategoryServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IRepository<Category>> _repoMock;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new CategoryProfile());
            });
            _mapper = config.CreateMapper();

            _repoMock = new Mock<IRepository<Category>>();
            _service = new CategoryService(_repoMock.Object, _mapper);
        }

        [Fact]
        public async Task CreateAsync_CreatesEntityAndReturnsDto()
        {
            var createDto = new CreateCategoryDto
            {
                Name = "TestCat",
                Description = "TestDesc"
            };

            var result = await _service.CreateAsync(createDto);

            Assert.NotNull(result);
            Assert.Equal("TestCat", result.Name);
            Assert.Equal("TestDesc", result.Description);
            _repoMock.Verify(r => r.AddAsync(It.Is<Category>(c =>
                c.Name == "TestCat" && c.Description == "TestDesc")), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedDtos()
        {
            var categories = new[]
            {
                new Category("A", "descA"),
                new Category("B", "descB")
            };
            _repoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(categories);

            var result = await _service.GetAllAsync();

            var list = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(result);
            Assert.Collection(list,
                dto => Assert.Equal("A", dto.Name),
                dto => Assert.Equal("B", dto.Name));
        }

        [Fact]
        public async Task GetByIdAsync_WhenExists_ReturnsDto()
        {
            var existing = new Category("Exists", "desc");
            _repoMock.Setup(r => r.GetByIdAsync(existing.Id))
                .ReturnsAsync(existing);

            var result = await _service.GetByIdAsync(existing.Id);

            Assert.NotNull(result);
            Assert.Equal(existing.Id, result!.Id);
            Assert.Equal("Exists", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Category?)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_WhenExists_UpdatesAndReturnsTrue()
        {
            var existing = new Category("OldName", "oldDesc");
            _repoMock.Setup(r => r.GetByIdAsync(existing.Id))
                .ReturnsAsync(existing);

            var updateDto = new UpdateCategoryDto
            {
                Name = "NewName",
                Description = "newDesc"
            };

            var result = await _service.UpdateAsync(existing.Id, updateDto);

            
            Assert.True(result);
            Assert.Equal("NewName", existing.Name);
            Assert.Equal("newDesc", existing.Description);
            _repoMock.Verify(r => r.Update(existing), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotExists_ReturnsFalse()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Category?)null);

            var updateDto = new UpdateCategoryDto
            {
                Name = "NameX",
                Description = "DescX"
            };

            var result = await _service.UpdateAsync(Guid.NewGuid(), updateDto);

            Assert.False(result);
            _repoMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Never);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenExists_DeletesAndReturnsTrue()
        {
            var existing = new Category("ToDelete", null);
            _repoMock.Setup(r => r.GetByIdAsync(existing.Id))
                .ReturnsAsync(existing);

            var result = await _service.DeleteAsync(existing.Id);

            Assert.True(result);
            _repoMock.Verify(r => r.Remove(existing), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotExists_ReturnsFalse()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Category?)null);

            var result = await _service.DeleteAsync(Guid.NewGuid());

            Assert.False(result);
            _repoMock.Verify(r => r.Remove(It.IsAny<Category>()), Times.Never);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
