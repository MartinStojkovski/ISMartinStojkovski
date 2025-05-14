using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;             
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApi;                                   
using Xunit;

namespace IntegrationTest
{
    public class CategoriesControllerTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CategoriesControllerTests(WebApplicationFactory<Program> factory)
        {
            var dbName = Guid.NewGuid().ToString();

            _client = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");

                    builder.UseSetting(
                        "applicationName",
                        typeof(Program).Assembly.GetName().Name);

                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.Single(
                            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                        services.Remove(descriptor);

                        services.AddDbContext<AppDbContext>(opts =>
                            opts.UseInMemoryDatabase(dbName));
                    });
                })
                .CreateClient();
        }

        [Fact]
        public async Task Get_ReturnsEmptyList_WhenNoCategories()
        {
            var response = await _client.GetAsync("/api/Categories");
            response.EnsureSuccessStatusCode();

            var list = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
            Assert.NotNull(list);
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenCategoryDoesNotExist()
        {
            var id = Guid.NewGuid();
            var response = await _client.GetAsync($"/api/Categories/{id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_CreatesCategory_ReturnsCreatedWithLocation()
        {
            var createDto = new CreateCategoryDto { Name = "NewCat", Description = "Desc" };
            var response = await _client.PostAsJsonAsync("/api/Categories", createDto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Contains("/api/Categories/", response.Headers.Location!.ToString());

            var created = await response.Content.ReadFromJsonAsync<CategoryDto>();
            Assert.NotNull(created);
            Assert.Equal("NewCat", created!.Name);
            Assert.Equal("Desc", created.Description);
            Assert.NotEqual(Guid.Empty, created.Id);
        }

        [Fact]
        public async Task GetById_ReturnsCategory_WhenExists()
        {
            var createDto = new CreateCategoryDto { Name = "Cat1" };
            var postResp = await _client.PostAsJsonAsync("/api/Categories", createDto);
            var created = await postResp.Content.ReadFromJsonAsync<CategoryDto>();

            var getResp = await _client.GetAsync($"/api/Categories/{created!.Id}");
            getResp.EnsureSuccessStatusCode();
            var fetched = await getResp.Content.ReadFromJsonAsync<CategoryDto>();

            Assert.Equal(created.Id, fetched!.Id);
            Assert.Equal("Cat1", fetched.Name);
        }

        [Fact]
        public async Task Put_UpdatesCategory_WhenExists()
        {
            var createDto = new CreateCategoryDto { Name = "Old", Description = "Desc" };
            var postResp = await _client.PostAsJsonAsync("/api/Categories", createDto);
            var created = await postResp.Content.ReadFromJsonAsync<CategoryDto>();

            var updateDto = new UpdateCategoryDto { Name = "New", Description = "Desc2" };
            var putResp = await _client.PutAsJsonAsync($"/api/Categories/{created!.Id}", updateDto);

            Assert.Equal(HttpStatusCode.NoContent, putResp.StatusCode);

            var getResp = await _client.GetAsync($"/api/Categories/{created.Id}");
            getResp.EnsureSuccessStatusCode();
            var updated = await getResp.Content.ReadFromJsonAsync<CategoryDto>();
            Assert.Equal("New", updated!.Name);
            Assert.Equal("Desc2", updated.Description);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_WhenCategoryDoesNotExist()
        {
            var updateDto = new UpdateCategoryDto { Name = "X", Description = "Y" };
            var resp = await _client.PutAsJsonAsync($"/api/Categories/{Guid.NewGuid()}", updateDto);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task Delete_DeletesCategory_WhenExists()
        {
            var createDto = new CreateCategoryDto { Name = "ToDelete" };
            var postResp = await _client.PostAsJsonAsync("/api/Categories", createDto);
            var created = await postResp.Content.ReadFromJsonAsync<CategoryDto>();

            var delResp = await _client.DeleteAsync($"/api/Categories/{created!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);

            var getResp = await _client.GetAsync($"/api/Categories/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResp.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenCategoryDoesNotExist()
        {
            var resp = await _client.DeleteAsync($"/api/Categories/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }
    }
}
