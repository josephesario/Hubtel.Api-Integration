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
    public class AddSimcardTypeControllerTest
    {
        private readonly HubtelWalletDbContext _context;
        private readonly SimcardTypeController _controller;

        public AddSimcardTypeControllerTest()
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


        #region AddUserType

        [Fact]
        public async Task AddSimCardType_NullSimCardType_ReturnsBadRequest()
        {
            // Arrange
            ISimcardType SimCardType = null!;

            // Act
            var result = await _controller.AddSimType(SimCardType!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task AddSimType_EmptySimCardTypeName_ReturnsBadRequest()
        {
            // Arrange
            var userType = new Mock<ISimcardType>();
            userType.SetupGet(u => u.Name).Returns(string.Empty);

            // Act
            var result = await _controller.AddSimType(userType.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }


        [Fact]
        public async Task AddSimCardType_ExistingUserType_ReturnsConflict()
        {
            // Arrange
            var simCardType = new Mock<ISimcardType>();
            simCardType.SetupGet(u => u.Name).Returns("vodafone");

            await _context.TSimcardTypes!.AddAsync(new TSimcardType { Name = "vodafone",CreatedAt = DateTime.Now, Id = new Guid()});
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.AddSimType(simCardType.Object);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(conflictResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status409Conflict, apiResponse.StatusCode);
        }

        [Fact]
        public async Task AddSimCardType_SuccessfulAddition_ReturnsOk()
        {
            // Arrange
            var simCardType = new Mock<ISimcardType>();
            simCardType.SetupGet(u => u.Name).Returns("vodafone");

            // Act
            var result = await _controller.AddSimType(simCardType.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<TSimcardType>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(StatusCodes.Status200OK, apiResponse.StatusCode);
        }

        [Fact]
        public async Task AddSimCardType_DbUpdateException_ReturnsInternalServerError()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<HubtelWalletDbContext>()
                .UseInMemoryDatabase(databaseName: "SimCardTestDb")
                .Options;
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var context = new HubtelWalletDbContext(options, configuration);
            var controller = new SimcardTypeController(context);

            var simCardType = new Mock<ISimcardType>();
            simCardType.SetupGet(u => u.Name).Returns("vodafone");


            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            await context.TSimcardTypes!.AddAsync(new TSimcardType { Name = "vodafone" });
            await context.SaveChangesAsync();

            var mockContext = new Mock<HubtelWalletDbContext>(options, configuration);
            mockContext.Setup(c => c.TSimcardTypes!.AddAsync(It.IsAny<TSimcardType>(), It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new DbUpdateException("An unexpected error occurred", new Exception("Inner exception message")));

            var controllerWithMock = new SimcardTypeController(mockContext.Object);

            var result = await controllerWithMock.AddSimType(simCardType.Object);

            var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(internalServerErrorResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status500InternalServerError, apiResponse.StatusCode);
            Assert.Contains("An unexpected error occurred", apiResponse.Message);
        }


        [Fact]
        public async Task AddSimCardType_UnexpectedException_ReturnsInternalServerError()
        {
            var options = new DbContextOptionsBuilder<HubtelWalletDbContext>()
                .UseInMemoryDatabase(databaseName: "SimCardTestDb")
                .Options;
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var mockContext = new Mock<HubtelWalletDbContext>(options, configuration);
            var controller = new SimcardTypeController(mockContext.Object);

            var simCardType = new Mock<ISimcardType>();
            simCardType.SetupGet(u => u.Name).Returns("vodafone");

            mockContext.Setup(c => c.TSimcardTypes!.AddAsync(It.IsAny<TSimcardType>(), It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await controller.AddSimType(simCardType.Object);

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