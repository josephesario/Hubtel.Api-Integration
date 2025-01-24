using System;
using System.Collections.Generic;
using System.Linq;
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
    public class GetAllCardTypeControllerTests
    {
        private readonly HubtelWalletDbContextExtended _context;
        private readonly CardTypeController _controller;

        public GetAllCardTypeControllerTests()
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
        public async Task GetAllCardTypes_WithCardTypes_ReturnsAllCardTypes()
        {
            var cardTypes = new List<TCardType>
            {
                new TCardType { Name = "visa" },
                new TCardType { Name = "master card" }
            };
            await _context.TCardTypes!.AddRangeAsync(cardTypes);
            await _context.SaveChangesAsync();

            var result = await _controller.GetAllCardTypes();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<IEnumerable<TCardType>>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(StatusCodes.Status200OK, apiResponse.StatusCode);
            Assert.Equal(2, apiResponse.Data.Count());
        }

        [Fact]
        public async Task GetAllCardTypes_NoCardTypes_ReturnsNotFound()
        {
            var result = await _controller.GetAllCardTypes();

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(notFoundResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status404NotFound, apiResponse.StatusCode);
        }
    }
}