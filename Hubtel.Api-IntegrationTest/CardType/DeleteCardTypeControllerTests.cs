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
using ViewModel.Interfaces;

namespace Hubtel.Api_IntegrationTest.CardType
{
    public class DeleteCardTypeControllerTests
    {
        private readonly HubtelWalletDbContextExtended _context;
        private readonly CardTypeController _controller;

        public DeleteCardTypeControllerTests()
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
        public async Task DeleteCardType_NullOrEmptyName_ReturnsBadRequest()
        {
            var result = await _controller.DeleteCardType(string.Empty);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
            Assert.Contains("Card type name is required", apiResponse.Errors);
        }

        [Fact]
        public async Task DeleteCardType_InvalidCardTypeName_ReturnsBadRequest()
        {
            var result = await _controller.DeleteCardType("invalidType");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
            Assert.Contains("Card type must be either 'visa' or 'master card'", apiResponse.Errors);
        }

        [Fact]
        public async Task DeleteCardType_SuccessfulDeletion_ReturnsOk()
        {
            var cardType = new TCardType { Name = "visa" };
            await _context.TCardTypes!.AddAsync(cardType);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteCardType("visa");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(StatusCodes.Status200OK, apiResponse.StatusCode);
            Assert.Null(_context.TCardTypes!.FirstOrDefault(u => u.Name == "visa"));
        }

        [Fact]
        public async Task DeleteCardType_CardTypeWithDependentData_ReturnsBadRequest()
        {
            // Arrange
            var cardType = new TCardType
            {
                Id = Guid.NewGuid(),
                Name = "Visa"
            };

            var userAccess = new TUserAccess
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
            };

            await _context.TCardTypes!.AddAsync(cardType);
            await _context.TCardAccountDetails!.AddAsync(new TCardAccountDetail
            {
                CardTypeId = cardType.Id,
                CardNumber = "03647283922",
                CreatedAt = DateTime.Now,
                Id = Guid.NewGuid(),
                UserAccessId = userAccess.Id,
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteCardType("Visa");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);

            // Case-insensitive assertion
            Assert.Contains(apiResponse.Errors,
                error => error.Equals("Card type with name 'visa' has dependent data", StringComparison.OrdinalIgnoreCase));
        }

    }
}