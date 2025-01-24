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

namespace Hubtel.Api_IntegrationTest.CardType
{
    public class GetCardTypeControllerTests
    {
        private readonly HubtelWalletDbContextExtended _context;
        private readonly CardTypeController _controller;

        public GetCardTypeControllerTests()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var options = new DbContextOptionsBuilder<HubtelWalletDbContextExtended>()
                .UseInMemoryDatabase(databaseName: "CardTypeTestDb")
                .Options;

            _context = new HubtelWalletDbContextExtended(options, configuration);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _controller = new CardTypeController(_context);
        }

        [Fact]
        public async Task GetCardType_NullName_ReturnsBadRequest()
        {
            var result = await _controller.GetCardTypeByName(null);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetCardType_EmptyName_ReturnsBadRequest()
        {
            var result = await _controller.GetCardTypeByName("");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetCardType_InvalidName_ReturnsBadRequest()
        {
            var result = await _controller.GetCardTypeByName("invalid");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetCardType_NonExistentCardType_ReturnsNotFound()
        {
            await _context.TCardTypes!.AddAsync(new TCardType { Name = "visa" });
            await _context.SaveChangesAsync();

            var result = await _controller.GetCardTypeByName("master card");

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(notFoundResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status404NotFound, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetCardType_ExistingCardType_ReturnsOk()
        {
            await _context.TCardTypes!.AddAsync(new TCardType { Name = "visa" });
            await _context.SaveChangesAsync();

            var result = await _controller.GetCardTypeByName("visa");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<TCardType>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(StatusCodes.Status200OK, apiResponse.StatusCode);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal("visa", apiResponse.Data.Name);
        }
    }
}