using System;
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

namespace Hubtel.Api_IntegrationTest.UserAccessControl
{
    public class RegisterTest
    {
        private readonly HubtelWalletDbContext _context;
        private readonly UserAccessController _controller;

        public RegisterTest()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var options = new DbContextOptionsBuilder<HubtelWalletDbContext>()
                .UseInMemoryDatabase(databaseName: "UserAccessTestDb")
                .Options;

            _context = new HubtelWalletDbContext(options, configuration);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _controller = new UserAccessController(_context, configuration);
        }

        [Fact]
        public async Task Register_NullUser_ReturnsBadRequest()
        {
            IUserAccess user = null!;
            var result = await _controller.Register(user!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
        }

        [Fact]
        public async Task Register_MissingEmailPhoneNumber_ReturnsBadRequest()
        {
            var user = new Mock<IUserAccess>();
            user.SetupGet(u => u.EmailPhoneNumber).Returns(null as string);
            user.SetupGet(u => u.UserSecret).Returns("ValidPass123");
            user.SetupGet(u => u.UserType).Returns("Admin");

            var result = await _controller.Register(user.Object);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
            Assert.Contains("Email Or Phone Number is required", apiResponse.Errors);
        }

        [Fact]
        public async Task Register_MissingPassword_ReturnsBadRequest()
        {
            var user = new Mock<IUserAccess>();
            user.SetupGet(u => u.EmailPhoneNumber).Returns("user@example.com");
            user.SetupGet(u => u.UserSecret).Returns(null as string);
            user.SetupGet(u => u.UserType).Returns("Admin");

            var result = await _controller.Register(user.Object);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
            Assert.Contains("Password is required", apiResponse.Errors);
        }

        [Fact]
        public async Task Register_MissingUserType_ReturnsBadRequest()
        {
            var user = new Mock<IUserAccess>();
            user.SetupGet(u => u.EmailPhoneNumber).Returns("user@example.com");
            user.SetupGet(u => u.UserSecret).Returns("ValidPass123");
            user.SetupGet(u => u.UserType).Returns(null as string);

            var result = await _controller.Register(user.Object);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, apiResponse.StatusCode);
            Assert.Contains("User Type is required", apiResponse.Errors);
        }

        [Fact]
        public async Task Register_UserTypeNotFound_ReturnsNotFound()
        {
            var user = new Mock<IUserAccess>();
            user.SetupGet(u => u.EmailPhoneNumber).Returns("user@example.com");
            user.SetupGet(u => u.UserSecret).Returns("ValidPass123");
            user.SetupGet(u => u.UserType).Returns("NonExistentType");

            var result = await _controller.Register(user.Object);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<string>>(notFoundResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal(StatusCodes.Status404NotFound, apiResponse.StatusCode);
            Assert.Contains("User Type not found", apiResponse.Errors);
        }

    }
}
