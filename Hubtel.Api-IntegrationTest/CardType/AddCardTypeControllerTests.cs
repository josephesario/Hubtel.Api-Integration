using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Hubtel.Api_Integration.Controllers;
using ViewModel.Data;
using ViewModel.Interfaces;
using dbContex.Models;

namespace Hubtel.Api_IntegrationTest.CardType
{
    public class AddCardTypeControllerTests
    {
        private readonly HubtelWalletDbContextExtended _context;
        private readonly CardTypeController _controller;

        public AddCardTypeControllerTests()
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
        public async Task AddCardType_NullCardType_ReturnsBadRequest()
        {
            ICardType cardType = null!;
            var result = await _controller.AddCardType(cardType!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task AddCardType_EmptyCardTypeName_ReturnsBadRequest()
        {
            var cardType = new Mock<ICardType>();
            cardType.SetupGet(c => c.Name).Returns(string.Empty);

            var result = await _controller.AddCardType(cardType.Object);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task AddCardType_InvalidCardType_ReturnsBadRequest()
        {
            var cardType = new Mock<ICardType>();
            cardType.SetupGet(c => c.Name).Returns("invalidType");

            var result = await _controller.AddCardType(cardType.Object);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
            Assert.Contains("Card must be either 'visa' or 'master card'", apiResponse.Errors);
        }

        [Fact]
        public async Task AddCardType_ExistingCardType_ReturnsConflict()
        {
            var cardType = new Mock<ICardType>();
            cardType.SetupGet(c => c.Name).Returns("visa");

            await _context.TCardTypes!.AddAsync(new TCardType { Name = "visa" });
            await _context.SaveChangesAsync();

            var result = await _controller.AddCardType(cardType.Object);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(conflictResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status409Conflict, apiResponse.StatusCode);
        }

        [Fact]
        public async Task AddCardType_SuccessfulAddition_ReturnsOk()
        {
            var cardType = new Mock<ICardType>();
            cardType.SetupGet(c => c.Name).Returns("visa");

            var result = await _controller.AddCardType(cardType.Object);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<TCardType>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(StatusCodes.Status200OK, apiResponse.StatusCode);
        }

        [Fact]
        public async Task AddCardType_UnexpectedException_ReturnsInternalServerError()
        {
            var options = new DbContextOptionsBuilder<HubtelWalletDbContextExtended>()
                .UseInMemoryDatabase(databaseName: "CardTypeTestDb")
                .Options;
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var mockContext = new Mock<HubtelWalletDbContextExtended>(options, configuration);
            var controller = new CardTypeController(mockContext.Object);

            var cardType = new Mock<ICardType>();
            cardType.SetupGet(c => c.Name).Returns("visa");

            mockContext.Setup(c => c.TCardTypes!.AddAsync(It.IsAny<TCardType>(), It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new Exception("Unexpected error"));

            var result = await controller.AddCardType(cardType.Object);

            var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(internalServerErrorResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status500InternalServerError, apiResponse.StatusCode);
            Assert.Contains("An unexpected error occurred", apiResponse.Message);
        }
    }
}