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
using System.Linq.Expressions;

namespace Hubtel.Api_IntegrationTest.UserType
{
    public class DeleteAccountTypeControllerTests
    {

        private readonly HubtelWalletDbContext _context;
        private readonly AccountTypeController _controller;

        public DeleteAccountTypeControllerTests()
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

        #region DeleteUserType

        [Fact]
        public async Task DeleteUserType_NullOrEmptyName_ReturnsBadRequest()
        {
            // Arrange
            string name = string.Empty;

            // Act
            var result = await _controller.DeleteAccountType(name);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
            Assert.Contains("User type name is required", apiResponse.Errors);
        }

        [Fact]
        public async Task DeleteUserType_InvalidUserTypeName_ReturnsBadRequest()
        {
            // Arrange
            string name = "invalidType";

            // Act
            var result = await _controller.DeleteAccountType(name);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
            Assert.Contains("User type must be either 'momo' or 'card'", apiResponse.Errors);
        }


        [Fact]
        public async Task DeleteUserType_SuccessfulDeletion_ReturnsOk()
        {
            // Arrange
            var userType = new TType { Name = "momo" };
            await _context.TTypes!.AddAsync(userType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteAccountType("momo");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(StatusCodes.Status200OK, 200);
            Assert.Null(_context.TTypes!.FirstOrDefault(u => u.Name == "momo"));
        }

        #endregion

    }
}
