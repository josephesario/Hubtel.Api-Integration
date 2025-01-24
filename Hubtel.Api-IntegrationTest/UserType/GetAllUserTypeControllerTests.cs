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

namespace Hubtel.Api_IntegrationTest.UserType
{
    public class GetAllUserTypeControllerTests
    {
        private readonly HubtelWalletDbContextExtended _context;
        private readonly UserTypeController _controller;

        public GetAllUserTypeControllerTests()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var options = new DbContextOptionsBuilder<HubtelWalletDbContextExtended>()
                .UseInMemoryDatabase(databaseName: "UserTypeTestDb")
                .Options;
            _context = new HubtelWalletDbContextExtended(options, configuration);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _controller = new UserTypeController(_context);
        }

        [Fact]
        public async Task GetAllUserTypes_WithUserTypes_ReturnsAllUserTypes()
        {
            // Arrange
            var userTypes = new List<TUserType>
            {
                new TUserType { Name = "momo" },
                new TUserType { Name = "card" }
            };
            await _context.TUserTypes!.AddRangeAsync(userTypes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAllUserTypes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<IEnumerable<TUserType>>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(StatusCodes.Status200OK, apiResponse.StatusCode);
            Assert.Equal(2, apiResponse.Data.Count());
        }

        [Fact]
        public async Task GetAllUserTypes_NoUserTypes_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetAllUserTypes();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(notFoundResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status404NotFound, apiResponse.StatusCode);
        }

        
    }
}