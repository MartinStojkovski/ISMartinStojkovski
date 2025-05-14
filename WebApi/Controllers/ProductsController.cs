using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi;
using Xunit;
using Infrastructure.Data;

namespace WebApi.Controllers
{
    public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProductsControllerTests(WebApplicationFactory<Program> factory)
        {
            var dbName = Guid.NewGuid().ToString();
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                
                    var descriptor = services.Single(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    services.Remove(descriptor);
                    services.AddDbContext<AppDbContext>(opts =>
                        opts.UseInMemoryDatabase(dbName));
                });
            }).CreateClient();
        }

        private async Task<Guid> CreateCategoryAsync()
        {
            var createCat = new CreateCategoryDto { Name = "CatX", Description = "DescX" };
            var resp = await _client.PostAsJsonAsync("/api/Categories", createCat);
            resp.EnsureSuccessStatusCode();
            var cat = await resp.Content.ReadFromJsonAsync<CategoryDto>();
            return cat!.Id;
        }

        [Fact]
        public async Task Crud_Workflow_Works()
        {
            var empty = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products");
            Assert.IsEmpty(empty);

            var catId = await CreateCategoryAsync();

            var createProd = new CreateProductDto
            {
                Name = "Prod1",
                Description = "A product",
                Price = 12.34m,
                CategoryId = catId
            };
            var postResp = await _client.PostAsJsonAsync("/api/Products", createProd);
            Assert.Equals(HttpStatusCode.Created, postResp.StatusCode);

            var prod = await postResp.Content.ReadFromJsonAsync<ProductDto>();
            Assert.Equals("Prod1", prod!.Name);
            Assert.Equals(catId, prod.CategoryId);

            var getResp = await _client.GetAsync($"/api/Products/{prod.Id}");
            Assert.Equals(HttpStatusCode.OK, getResp.StatusCode);
            var fetched = await getResp.Content.ReadFromJsonAsync<ProductDto>();
            Assert.Equals(prod.Id, fetched!.Id);

            var updateProd = new UpdateProductDto
            {
                Name = "Prod2",
                Description = "Updated",
                Price = 99.99m,
                CategoryId = catId
            };
            var putResp = await _client.PutAsJsonAsync($"/api/Products/{prod.Id}", updateProd);
            Assert.Equals(HttpStatusCode.NoContent, putResp.StatusCode);

            var updated = await _client.GetFromJsonAsync<ProductDto>($"/api/Products/{prod.Id}");
            Assert.Equals("Prod2", updated.Name);
            Assert.Equals(99.99m, updated.Price);

            var delResp = await _client.DeleteAsync($"/api/Products/{prod.Id}");
            Assert.Equals(HttpStatusCode.NoContent, delResp.StatusCode);
            var getAfterDelete = await _client.GetAsync($"/api/Products/{prod.Id}");
            Assert.Equals(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
        }
    }
}
