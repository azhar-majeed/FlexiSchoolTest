using NUnit.Framework;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Flexischools.Infrastructure.Data;
using Flexischools.Application.DTOs;

namespace Flexischools.IntegrationTests.Controllers;

[TestFixture]
public class OrdersControllerIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private FlexischoolsDbContext _context;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real DbContext
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<FlexischoolsDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add InMemory database
                    services.AddDbContext<FlexischoolsDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
                    });
                });
            });

        _client = _factory.CreateClient();
        
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<FlexischoolsDbContext>();
        _context.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
    }



    [Test]
    public async Task GetOrder_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("title").GetString().Should().Be("Order Not Found");
    }

    [Test]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
