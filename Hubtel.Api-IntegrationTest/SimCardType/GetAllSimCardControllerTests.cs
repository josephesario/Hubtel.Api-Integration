using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using Hubtel.Api_Integration.Controllers;
using ViewModel.Data;
using dbContex.Models;
using Moq;
using ViewModel.Interfaces;

namespace Hubtel.Api_IntegrationTest.UserType
{
    public class GetAllSimCardControllerTests
    {
        private readonly HubtelWalletDbContext _context;
        private readonly SimcardTypeController _controller;

        public GetAllSimCardControllerTests()
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
        public async Task GetAllSimCardTypes_WithSimCardTypes_ReturnsAllSimCardTypes()
        {
            // Arrange
            var SimCardTypes = new List<TSimcardType>
            {
                new TSimcardType { Name = "vodafone",CreatedAt = DateTime.Now,Id = new Guid() },
                new TSimcardType { Name = "airteltigo",CreatedAt = DateTime.Now,Id = new Guid() },
                new TSimcardType { Name = "mtn",CreatedAt = DateTime.Now,Id = new Guid() }
            };
            await _context.TSimcardTypes!.AddRangeAsync(SimCardTypes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAllSimTypes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<IEnumerable<ISimcardType>>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(StatusCodes.Status200OK, apiResponse.StatusCode);
            Assert.Equal(3, apiResponse.Data.Count());
        }

        [Fact]
        public async Task GetAllSimCardTypes_NoSimCardTypes_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetAllSimTypes();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(notFoundResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status404NotFound, apiResponse.StatusCode);
        }

        
    }
}