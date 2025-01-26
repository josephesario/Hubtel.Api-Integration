using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using dbContex.Models;
using Hubtel.Api_Integration.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ViewModel.Data;
using Xunit;

namespace Hubtel.Api_IntegrationTest.WalletAccount
{
    public class GetAllCardAccountDetailsControllerTest
    {
        private readonly HubtelWalletDbContext _context;
        private readonly WalletAccountDetailController _controller;

        public GetAllCardAccountDetailsControllerTest()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            var options = new DbContextOptionsBuilder<HubtelWalletDbContext>()
                .UseInMemoryDatabase(databaseName: "WalletAccountTestDb")
                .Options;

            _context = new HubtelWalletDbContext(options, configuration);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _controller = new WalletAccountDetailController(_context, configuration);
        }

        private async Task<(TUserAccess userAccess, TUserProfile userProfile, TType accountType, TCardType cardType)> SeedTestData()
        {
            // Seed user access
            var userAccess = new TUserAccess
            {
                Id = Guid.NewGuid(),
                EmailPhoneNumber = "test@example.com"
            };
            await _context.TUserAccesses.AddAsync(userAccess);

            // Seed user profile
            var userProfile = new TUserProfile
            {
                Id = Guid.NewGuid(),
                UserAccessId = userAccess.Id,
                LegalName = "Test User",
                EmailPhone = "test@example.com",
                IdentityCardNumber = "TEST123456"
            };
            await _context.TUserProfiles.AddAsync(userProfile);

            // Seed account type
            var accountType = new TType
            {
                Id = Guid.NewGuid(),
                Name = "card"
            };
            await _context.TTypes.AddAsync(accountType);

            // Seed card type
            var cardType = new TCardType
            {
                Id = Guid.NewGuid(),
                Name = "MasterCard"
            };
            await _context.TCardTypes.AddAsync(cardType);

            await _context.SaveChangesAsync();

            return (userAccess, userProfile, accountType, cardType);
        }

        private void SetupUserContext(string emailPhone)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, emailPhone)
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetAllCardAccountDetails_ValidUser_ReturnsOkWithAccountDetails()
        {
            // Arrange
            var (userAccess, userProfile, accountType, cardType) = await SeedTestData();
            SetupUserContext("test@example.com");

            // Create multiple wallet account details
            var walletAccountDetails = new List<TWalletAccountDetail>
            {
                new TWalletAccountDetail
                {
                    Id = Guid.NewGuid(),
                    AccountNumber = "1234567890123456",
                    UserProfileId = userProfile.Id,
                    UserAccessId = userAccess.Id,
                    CardTypeId = cardType.Id,
                    AccountTypeId = accountType.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new TWalletAccountDetail
                {
                    Id = Guid.NewGuid(),
                    AccountNumber = "6543210987654321",
                    UserProfileId = userProfile.Id,
                    UserAccessId = userAccess.Id,
                    CardTypeId = cardType.Id,
                    AccountTypeId = accountType.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };
            await _context.TWalletAccountDetails.AddRangeAsync(walletAccountDetails);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAllAccountDetails();

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var apiResponse = Assert.IsType<ApiResponse<List<WalletAccountDetailOut>>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(2, apiResponse.Data.Count);
            Assert.Contains(apiResponse.Data, d => d.AccountNumber == "1234567890123456");
            Assert.Contains(apiResponse.Data, d => d.AccountNumber == "6543210987654321");
        }

        [Fact]
        public async Task GetAllCardAccountDetails_NoAccountsForUser_ReturnsEmptyList()
        {
            // Arrange
            var (userAccess, userProfile, _, _) = await SeedTestData();
            SetupUserContext("test@example.com");

            // Act
            var result = await _controller.GetAllAccountDetails();

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var apiResponse = Assert.IsType<ApiResponse<List<WalletAccountDetailOut>>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Empty(apiResponse.Data);
        }


        [Fact]
        public async Task GetAllCardAccountDetails_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            await SeedTestData();

            // Explicitly set up a context with no authentication
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // No user set
            };

            // Act
            var result = await _controller.GetAllAccountDetails();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);

            var apiResponse = Assert.IsType<ApiResponse<string>>(unauthorizedResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Contains("not authenticated", apiResponse.Message);
        }


        [Fact]
        public async Task GetAllCardAccountDetails_UserProfileNotFound_ReturnsBadRequest()
        {
            // Arrange
            var userAccess = new TUserAccess
            {
                Id = Guid.NewGuid(),
                EmailPhoneNumber = "test@example.com"
            };
            await _context.TUserAccesses.AddAsync(userAccess);
            await _context.SaveChangesAsync();

            SetupUserContext("test@example.com");

            // Act
            var result = await _controller.GetAllAccountDetails();

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

            var apiResponse = Assert.IsType<ApiResponse<string>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Contains("User profile not found", apiResponse.Message);
        }
    }
}