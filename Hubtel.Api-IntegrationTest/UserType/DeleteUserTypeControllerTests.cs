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
    public class DeleteUserTypeControllerTests
    {

        private readonly HubtelWalletDbContextExtended _context;
        private readonly UserTypeController _controller;

        public DeleteUserTypeControllerTests()
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

        #region DeleteUserType

        [Fact]
        public async Task DeleteUserType_NullOrEmptyName_ReturnsBadRequest()
        {
            // Arrange
            string name = string.Empty;

            // Act
            var result = await _controller.DeleteUserType(name);

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
            var result = await _controller.DeleteUserType(name);

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
            var userType = new TUserType { Name = "momo" };
            await _context.TUserTypes!.AddAsync(userType);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteUserType("momo");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(StatusCodes.Status200OK, apiResponse.StatusCode);
            Assert.Null(_context.TUserTypes!.FirstOrDefault(u => u.Name == "momo"));
        }


        [Fact]
        public async Task DeleteUserType_UserTypeWithDependentData_ReturnsBadRequest()
        {
            // Arrange
            var userType = new TUserType
            {
                Id = Guid.NewGuid(),
                Name = "momo"
            };
            await _context.TUserTypes!.AddAsync(userType);
            await _context.TUserAccesses!.AddAsync(new TUserAccess
            {
                UserTypeId = userType.Id,
                EmailPhoneNumber = "test@example.com",
                Id = Guid.NewGuid() // Add Id for TUserAccess
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteUserType("momo");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
            Assert.Contains($"User type with name 'momo' has dependent data", apiResponse.Errors);
        }

        #endregion

    }
}
