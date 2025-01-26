using dbContex.Models;
using Hubtel.Api_Integration.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;


namespace Hubtel.Api_IntegrationTest.WalletAccount
{

    public class DeleteAccountDetailControllerTest
    {
        private readonly HubtelWalletDbContext _context;
        private readonly WalletAccountDetailController _controller;

        public DeleteAccountDetailControllerTest()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            var options = new DbContextOptionsBuilder<HubtelWalletDbContext>()
                .UseInMemoryDatabase(databaseName: "WalletAccountDeleteTestDb")
                .Options;

            _context = new HubtelWalletDbContext(options, configuration);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _controller = new WalletAccountDetailController(_context, configuration);
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

        private async Task<TUserAccess> SeedTestData()
        {
            var userAccess = new TUserAccess
            {
                Id = Guid.NewGuid(),
                EmailPhoneNumber = "test@example.com"
            };
            await _context.TUserAccesses.AddAsync(userAccess);

            var userProfile = new TUserProfile
            {
                Id = Guid.NewGuid(),
                UserAccessId = userAccess.Id,
                LegalName = "Test User",
                EmailPhone = "test@example.com",
                IdentityCardNumber = "TEST123456"
            };
            await _context.TUserProfiles.AddAsync(userProfile);

            var cardType = new TCardType
            {
                Id = Guid.NewGuid(),
                Name = "Master Card"
            };
            await _context.TCardTypes.AddAsync(cardType);

            var accountType = new TType
            {
                Id = Guid.NewGuid(),
                Name = "card"
            };
            await _context.TTypes.AddAsync(accountType);

            await _context.SaveChangesAsync();
            return userAccess;
        }

        [Fact]
        public async Task DeleteAccountDetail_ValidAccountNumber_ShouldReturnOk()
        {
            // Arrange
            var userAccess = await SeedTestData();
            SetupUserContext(userAccess.EmailPhoneNumber);

            var userProfile = await _context.TUserProfiles.FirstAsync(u => u.UserAccessId == userAccess.Id);
            var cardType = await _context.TCardTypes.FirstAsync();
            var accountType = await _context.TTypes.FirstAsync(t => t.Name == "card");

            var walletAccount = new TWalletAccountDetail
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890123456",
                UserProfileId = userProfile.Id,
                CardTypeId = cardType.Id,
                AccountTypeId = accountType.Id
            };
            await _context.TWalletAccountDetails.AddAsync(walletAccount);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteAccountDetailByAccountNumber("1234567890123456");

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAccountDetail_NonExistentAccountNumber_ShouldReturnNotFound()
        {
            // Arrange
            var userAccess = await SeedTestData();
            SetupUserContext(userAccess.EmailPhoneNumber);

            // Act
            var result = await _controller.DeleteAccountDetailByAccountNumber("9999999999");

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAccountDetail_DifferentUserAccount_ShouldReturnUnauthorized()
        {
            // Arrange
            var userAccess1 = await SeedTestData();
            var userAccess2 = await SeedTestData();
            SetupUserContext(userAccess1.EmailPhoneNumber);

            var userProfile2 = await _context.TUserProfiles.FirstAsync(u => u.UserAccessId == userAccess2.Id);
            var cardType = await _context.TCardTypes.FirstAsync();
            var accountType = await _context.TTypes.FirstAsync(t => t.Name == "card");

            var walletAccount = new TWalletAccountDetail
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890123456",
                UserProfileId = userProfile2.Id,
                CardTypeId = cardType.Id,
                AccountTypeId = accountType.Id
            };
            await _context.TWalletAccountDetails.AddAsync(walletAccount);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteAccountDetailByAccountNumber("1234567890123456");

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAccountDetail_EmptyAccountNumber_ShouldReturnNotFound()
        {
            // Arrange
            var userAccess = await SeedTestData();
            SetupUserContext(userAccess.EmailPhoneNumber);

            // Act
            var result = await _controller.DeleteAccountDetailByAccountNumber(string.Empty);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAccountDetail_UnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            await SeedTestData();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.DeleteAccountDetailByAccountNumber("1234567890123456");

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        }
    }
}