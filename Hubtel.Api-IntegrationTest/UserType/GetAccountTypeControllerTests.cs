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


namespace Hubtel.Api_IntegrationTest.UserType
{
    public class GetAccountTypeControllerTests
    {
        private readonly HubtelWalletDbContext _context;
        private readonly AccountTypeController _controller;

        public GetAccountTypeControllerTests()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var options = new DbContextOptionsBuilder<HubtelWalletDbContext>()
                .UseInMemoryDatabase(databaseName: "UserTypeTestDb")
                .Options;

            _context = new HubtelWalletDbContext(options, configuration);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _controller = new AccountTypeController(_context);
        }

        [Fact]
        public async Task GetUserType_NullName_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetAccountTypeByName(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetUserType_EmptyName_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetAccountTypeByName("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetUserType_InvalidName_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetAccountTypeByName("invalid");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetUserType_NonExistentUserType_ReturnsNotFound()
        {
            // Arrange
            await _context.TTypes!.AddAsync(new TType { Name = "momo" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAccountTypeByName("card");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(notFoundResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status404NotFound, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetUserType_ExistingUserType_ReturnsOk()
        {
            // Arrange
            await _context.TTypes!.AddAsync(new TType { Name = "momo" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAccountTypeByName("momo");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<TType>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(StatusCodes.Status200OK, apiResponse.StatusCode);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal("momo", apiResponse.Data.Name);
        }
    }
}