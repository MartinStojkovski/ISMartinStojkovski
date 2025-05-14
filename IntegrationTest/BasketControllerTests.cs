using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApi;
using Xunit;

namespace IntegrationTest
{
    public class BasketControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BasketControllerTests(WebApplicationFactory<Program> factory)
        {
            var dbName = Guid.NewGuid().ToString();

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                    services.RemoveAll(typeof(AppDbContext));

                    services.AddDbContext<AppDbContext>(opts =>
                        opts.UseInMemoryDatabase(dbName));
                });
            });

            _client = _factory.CreateClient();
        }

        private async Task<Guid> SeedOneProductWithStock()
        {
            using var scope = _factory.Services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cat = new Category("CatX", null);
            ctx.Categories.Add(cat);
            await ctx.SaveChangesAsync();

            var prod = new Product
            {
                Name = "Widget",
                Description = "Test widget",
                Price = 100m,
                CategoryId = cat.Id,
                Category = cat
            };
            ctx.Products.Add(prod);
            await ctx.SaveChangesAsync();

            var stock = new Stock
            {
                ProductId = prod.Id,
                Quantity = 5,
                LastUpdated = DateTime.UtcNow
            };
            ctx.Stocks.Add(stock);
            await ctx.SaveChangesAsync();

            return prod.Id;
        }

        [Fact]
        public async Task Discount_Applies5PercentOnDuplicates()
        {
            var prodId = await SeedOneProductWithStock();

            var basket = new List<BasketItemDto>
            {
                new BasketItemDto { ProductId = prodId, Quantity = 2 }
            };

            var response = await _client.PostAsJsonAsync("/api/Basket/discount", basket);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<DiscountResultDto>();
            Assert.NotNull(result);
            Assert.Equal(5m, result!.TotalDiscount);
        }
    }
}
