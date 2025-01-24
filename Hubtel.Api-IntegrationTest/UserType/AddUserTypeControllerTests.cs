using System;
using System.Linq.Expressions;
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
using Microsoft.Extensions.Options;
using Castle.Core.Configuration;

namespace Hubtel.Api_IntegrationTest.UserType
{
    public class AddUserTypeControllerTests
    {
        private readonly HubtelWalletDbContextExtended _context;
        private readonly UserTypeController _controller;

        public AddUserTypeControllerTests()
        {
            // Create a new in-memory configuration
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            // Configure the DbContext to use InMemory database
            var options = new DbContextOptionsBuilder<HubtelWalletDbContextExtended>()
                .UseInMemoryDatabase(databaseName: "UserTypeTestDb")
                .Options;

            // Create the DbContext with the configuration
            _context = new HubtelWalletDbContextExtended(options, configuration);
            _context.Database.EnsureDeleted(); // Clear the database before each test
            _context.Database.EnsureCreated(); // Create the database schema
            _controller = new UserTypeController(_context);
        }


        #region AddUserType

        [Fact]
        public async Task AddUserType_NullUserType_ReturnsBadRequest()
        {
            // Arrange
            IUserType userType = null!;

            // Act
            var result = await _controller.AddUserType(userType!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task AddUserType_EmptyUserTypeName_ReturnsBadRequest()
        {
            // Arrange
            var userType = new Mock<IUserType>();
            userType.SetupGet(u => u.Name).Returns(string.Empty);

            // Act
            var result = await _controller.AddUserType(userType.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task AddUserType_InvalidUserType_ReturnsBadRequest()
        {
            // Arrange
            var userType = new Mock<IUserType>();
            userType.SetupGet(u => u.Name).Returns("invalidType");

            // Act
            var result = await _controller.AddUserType(userType.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
            Assert.Contains("User type must be either 'momo' or 'card'", apiResponse.Errors);
        }

        [Fact]
        public async Task AddUserType_ExistingUserType_ReturnsConflict()
        {
            // Arrange
            var userType = new Mock<IUserType>();
            userType.SetupGet(u => u.Name).Returns("momo");

            await _context.TUserTypes!.AddAsync(new TUserType { Name = "momo" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.AddUserType(userType.Object);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(conflictResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status409Conflict, apiResponse.StatusCode);
        }

        [Fact]
        public async Task AddUserType_SuccessfulAddition_ReturnsOk()
        {
            // Arrange
            var userType = new Mock<IUserType>();
            userType.SetupGet(u => u.Name).Returns("momo");

            // Act
            var result = await _controller.AddUserType(userType.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<TUserType>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(StatusCodes.Status200OK, apiResponse.StatusCode);
        }

        [Fact]
        public async Task AddUserType_DbUpdateException_ReturnsInternalServerError()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<HubtelWalletDbContextExtended>()
                .UseInMemoryDatabase(databaseName: "UserTypeTestDb")
                .Options;
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var context = new HubtelWalletDbContextExtended(options, configuration);
            var controller = new UserTypeController(context);

            var userType = new Mock<IUserType>();
            userType.SetupGet(u => u.Name).Returns("momo");


            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            await context.TUserTypes!.AddAsync(new TUserType { Name = "momo" });
            await context.SaveChangesAsync();

            var mockContext = new Mock<HubtelWalletDbContextExtended>(options, configuration);
            mockContext.Setup(c => c.TUserTypes!.AddAsync(It.IsAny<TUserType>(), It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new DbUpdateException("An unexpected error occurred", new Exception("Inner exception message")));

            var controllerWithMock = new UserTypeController(mockContext.Object);

            var result = await controllerWithMock.AddUserType(userType.Object);

            var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(internalServerErrorResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status500InternalServerError, apiResponse.StatusCode);
            Assert.Contains("An unexpected error occurred", apiResponse.Message);
        }


        [Fact]
        public async Task AddUserType_UnexpectedException_ReturnsInternalServerError()
        {
            var options = new DbContextOptionsBuilder<HubtelWalletDbContextExtended>()
                .UseInMemoryDatabase(databaseName: "UserTypeTestDb")
                .Options;
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var mockContext = new Mock<HubtelWalletDbContextExtended>(options, configuration);
            var controller = new UserTypeController(mockContext.Object);

            var userType = new Mock<IUserType>();
            userType.SetupGet(u => u.Name).Returns("momo");

            mockContext.Setup(c => c.TUserTypes!.AddAsync(It.IsAny<TUserType>(), It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await controller.AddUserType(userType.Object);

            // Assert
            var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(internalServerErrorResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status500InternalServerError, apiResponse.StatusCode);
            Assert.Contains("An unexpected error occurred", apiResponse.Message);
        }

        #endregion



    }
}