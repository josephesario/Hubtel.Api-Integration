using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Hubtel.Api_IntegrationTest
{


    public class LoginTests
    {
        private readonly HubtelWalletDbContextExtended _context;
        private readonly UserAccessController _controller;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<DbSet<TUserAccess>> _mockUserAccessDbSet;

        public LoginTests()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var options = new DbContextOptionsBuilder<HubtelWalletDbContextExtended>()
                .UseInMemoryDatabase(databaseName: "UserAccessTestDb")
                .Options;

            _context = new HubtelWalletDbContextExtended(options, configuration);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _controller = new UserAccessController(_context, configuration);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockUserAccessDbSet = new Mock<DbSet<TUserAccess>>();
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenLoginAccessIsNull()
        {
            // Arrange
            ILogin loginAccess = null;

            // Act
            var result = await _controller.Login(loginAccess);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal("Invalid Credentials", apiResponse.Message);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenEmailPhoneNumberIsEmpty()
        {
            // Arrange
            var loginAccess = new Mock<ILogin>();
            loginAccess.Setup(l => l.EmailPhoneNumber).Returns(string.Empty);
            loginAccess.Setup(l => l.UserSecret).Returns("password");

            // Act
            var result = await _controller.Login(loginAccess.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal("Email Or Phone Number is required", apiResponse.Errors.First());
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenUserSecretIsEmpty()
        {
            // Arrange
            var loginAccess = new Mock<ILogin>();
            loginAccess.Setup(l => l.EmailPhoneNumber).Returns("test@example.com");
            loginAccess.Setup(l => l.UserSecret).Returns(string.Empty);

            // Act
            var result = await _controller.Login(loginAccess.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal("Password is required", apiResponse.Errors.First());
        }

        

    }
}