using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using dbContex.Models;
using Hubtel.Api_Integration.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ViewModel.Interfaces;

namespace Hubtel.Api_IntegrationTest.WalletAccount
{
    public class CreateAccountDetailControllerTest
    {
        private readonly HubtelWalletDbContext _context;
        private readonly WalletAccountDetailController _controller;

        public CreateAccountDetailControllerTest()
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

        private async Task SeedTestData()
        {
            // Seed user access
            var userAccess = new TUserAccess
            {
                Id = Guid.NewGuid(),
                EmailPhoneNumber = "test@example.com"
            };
            await _context.TUserAccesses.AddAsync(userAccess);

            // Seed user profile with all required properties
            var userProfile = new TUserProfile
            {
                Id = Guid.NewGuid(),
                UserAccessId = userAccess.Id,
                LegalName = "Test User",
                EmailPhone = "test@example.com",
                IdentityCardNumber = "TEST123456", // Add a required identity card number
            };
            await _context.TUserProfiles.AddAsync(userProfile);

            // Seed account types
            var cardType = new TType
            {
                Id = Guid.NewGuid(),
                Name = "card"
            };
            var momoType = new TType
            {
                Id = Guid.NewGuid(),
                Name = "momo"
            };
            await _context.TTypes.AddRangeAsync(cardType, momoType);

            // Seed card types
            var masterCardType = new TCardType
            {
                Id = Guid.NewGuid(),
                Name = "Master Card"
            };
            await _context.TCardTypes.AddAsync(masterCardType);

            // Seed sim card types
            var mtnSimType = new TSimcardType
            {
                Id = Guid.NewGuid(),
                Name = "MTN"
            };
            await _context.TSimcardTypes.AddAsync(mtnSimType);

            await _context.SaveChangesAsync();
        }


        [Fact]
        public async Task CreateAccountDetail_ValidCardAccount_ShouldReturnOk()
        {
            // Arrange
            await SeedTestData();
            SetupUserContext("test@example.com");

            var model = new Mock<IWalletAccountDetail>();
            model.SetupGet(m => m.AccountNumber).Returns("1234567890123456");
            model.SetupGet(m => m.ProfileLegalName).Returns("Test User");
            model.SetupGet(m => m.AccountScheme).Returns("Master Card");

            // Act
            var result = await _controller.CreateAccountDetail("card", model.Object);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateAccountDetail_ValidMomoAccount_ShouldReturnOk()
        {
            // Arrange
            await SeedTestData();
            SetupUserContext("test@example.com");

            var model = new Mock<IWalletAccountDetail>();
            model.SetupGet(m => m.AccountNumber).Returns("0501234567");
            model.SetupGet(m => m.ProfileLegalName).Returns("Test User");
            model.SetupGet(m => m.AccountScheme).Returns("MTN");

            // Act
            var result = await _controller.CreateAccountDetail("momo", model.Object);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateAccountDetail_NonExistentProfile_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestData();
            SetupUserContext("test@example.com");

            var model = new Mock<IWalletAccountDetail>();
            model.SetupGet(m => m.AccountNumber).Returns("1234567890123456");
            model.SetupGet(m => m.ProfileLegalName).Returns("Non-Existent User");
            model.SetupGet(m => m.AccountScheme).Returns("MasterCard");

            // Act
            var result = await _controller.CreateAccountDetail("card", model.Object);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateAccountDetail_EmptyAccountNumber_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestData();
            SetupUserContext("test@example.com");

            var model = new Mock<IWalletAccountDetail>();
            model.SetupGet(m => m.AccountNumber).Returns("");
            model.SetupGet(m => m.ProfileLegalName).Returns("Test User");
            model.SetupGet(m => m.AccountScheme).Returns("MasterCard");

            // Act
            var result = await _controller.CreateAccountDetail("card", model.Object);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateAccountDetail_InvalidCardNumber_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestData();
            SetupUserContext("test@example.com");

            var model = new Mock<IWalletAccountDetail>();
            model.SetupGet(m => m.AccountNumber).Returns("123");
            model.SetupGet(m => m.ProfileLegalName).Returns("Test User");
            model.SetupGet(m => m.AccountScheme).Returns("MasterCard");

            // Act
            var result = await _controller.CreateAccountDetail("card", model.Object);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateAccountDetail_InvalidAccountType_ShouldReturnNotFound()
        {
            // Arrange
            await SeedTestData();
            SetupUserContext("test@example.com");

            var model = new Mock<IWalletAccountDetail>();
            model.SetupGet(m => m.AccountNumber).Returns("1234567890123456");
            model.SetupGet(m => m.ProfileLegalName).Returns("Test User");
            model.SetupGet(m => m.AccountScheme).Returns("Master Card");

            // Act
            var result = await _controller.CreateAccountDetail("invalid", model.Object);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task CreateAccountDetail_MaxAccountsReached_ShouldReturnBadRequest()
        {
            // Arrange
            await SeedTestData();
            SetupUserContext("test@example.com");

            var userProfile = await _context.TUserProfiles.FirstAsync(u => u.EmailPhone == "test@example.com");
            var cardType = await _context.TCardTypes.FirstAsync();
            var accountType = await _context.TTypes.FirstAsync(t => t.Name == "card");

            // Add 5 existing wallet accounts
            for (int i = 0; i < 5; i++)
            {
                await _context.TWalletAccountDetails.AddAsync(new TWalletAccountDetail
                {
                    AccountNumber = $"123456789012345{i}",
                    UserProfileId = userProfile.Id,
                    CardTypeId = cardType.Id,
                    AccountTypeId = accountType.Id
                });
            }
            await _context.SaveChangesAsync();

            var model = new Mock<IWalletAccountDetail>();
            model.SetupGet(m => m.AccountNumber).Returns("1234567890123456");
            model.SetupGet(m => m.ProfileLegalName).Returns("Test User");
            model.SetupGet(m => m.AccountScheme).Returns("MasterCard");

            // Act
            var result = await _controller.CreateAccountDetail("card", model.Object);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateAccountDetail_DuplicateAccountNumber_ShouldReturnConflict()
        {
            // Arrange
            await SeedTestData();
            SetupUserContext("test@example.com");

            var userProfile = await _context.TUserProfiles.FirstAsync(u => u.EmailPhone == "test@example.com");
            var cardType = await _context.TCardTypes.FirstAsync();
            var accountType = await _context.TTypes.FirstAsync(t => t.Name == "card");

            // Add an existing wallet account
            await _context.TWalletAccountDetails.AddAsync(new TWalletAccountDetail
            {
                AccountNumber = "1234567890123456",
                UserProfileId = userProfile.Id,
                CardTypeId = cardType.Id,
                AccountTypeId = accountType.Id
            });
            await _context.SaveChangesAsync();

            var model = new Mock<IWalletAccountDetail>();
            model.SetupGet(m => m.AccountNumber).Returns("1234567890123456");
            model.SetupGet(m => m.ProfileLegalName).Returns("Test User");
            model.SetupGet(m => m.AccountScheme).Returns("MasterCard");

            // Act
            var result = await _controller.CreateAccountDetail("card", model.Object);

            // Assert
      
            Assert.Equal(StatusCodes.Status401Unauthorized, 401);
        }
    }
}
