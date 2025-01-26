using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using Hubtel.Api_Integration.Controllers;
using ViewModel.Data;
using dbContex.Models;


namespace Hubtel.Api_IntegrationTest.GetSimCardTypeControllerTests
{
    public class GetSimCardTypeControllerTests
    {
        private readonly HubtelWalletDbContext _context;
        private readonly SimcardTypeController _controller;

        public GetSimCardTypeControllerTests()
        {
            // Create a new in-memory configuration
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            // Configure the DbContext to use InMemory database
            var options = new DbContextOptionsBuilder<HubtelWalletDbContext>()
                .UseInMemoryDatabase(databaseName: "SimCardTestDb")
                .Options;

            // Create the DbContext with the configuration
            _context = new HubtelWalletDbContext(options, configuration);
            _context.Database.EnsureDeleted(); // Clear the database before each test
            _context.Database.EnsureCreated(); // Create the database schema
            _controller = new SimcardTypeController(_context);
        }

        [Fact]
        public async Task GetSimCardType_NullName_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetSimTypeByName(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetSimCardType_EmptyName_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetSimTypeByName("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetSimCardType_InvalidName_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetSimTypeByName("invalid");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetSimCardType_NonExistentSimcardTypes_ReturnsNotFound()
        {
            // Arrange
            await _context.TSimcardTypes!.AddAsync(new TSimcardType { Name = "vodafone" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetSimTypeByName("mtn");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(notFoundResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status404NotFound, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetSimCardType_ExistingSimcardTypes_ReturnsOk()
        {
            // Arrange
            await _context.TSimcardTypes!.AddAsync(new TSimcardType { Name = "vodafone" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetSimTypeByName("vodafone");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<SimcardType>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(StatusCodes.Status200OK, apiResponse.StatusCode);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal("vodafone", apiResponse.Data.Name);
        }
    }
}