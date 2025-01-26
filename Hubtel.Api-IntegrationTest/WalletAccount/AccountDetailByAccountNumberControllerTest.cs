﻿using System;
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
using ViewModel.Data;
using ViewModel.Interfaces;

namespace Hubtel.Api_IntegrationTest.WalletAccount
{
    public class AccountDetailByAccountNumberControllerTest
    {
        private readonly HubtelWalletDbContext _context;
        private readonly WalletAccountDetailController _controller;

        public AccountDetailByAccountNumberControllerTest()
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

            // Seed user profile with required properties
            var userProfile = new TUserProfile
            {
                Id = Guid.NewGuid(),
                UserAccessId = userAccess.Id,
                LegalName = "Test User",
                EmailPhone = "test@example.com",
                IdentityCardNumber = "TEST123456",

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
        public async Task AccountDetailByAccountNumber_ValidAccount_ReturnsOk()
        {
            // Arrange
            var (userAccess, userProfile, accountType, cardType) = await SeedTestData();
            SetupUserContext("test@example.com");

            // Create wallet account detail
            var walletAccountDetail = new TWalletAccountDetail
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890123456",
                UserProfileId = userProfile.Id,
                UserAccessId = userAccess.Id,
                CardTypeId = cardType.Id,
                AccountTypeId = accountType.Id
            };
            await _context.TWalletAccountDetails.AddAsync(walletAccountDetail);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.AccountDetailByAccountNumber("1234567890123456");

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var apiResponse = Assert.IsType<ApiResponse<IWalletAccountDetail>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal("1234567890123456", apiResponse.Data.AccountNumber);
        }

        [Fact]
        public async Task AccountDetailByAccountNumber_NonExistentAccount_ReturnsNotFound()
        {
            // Arrange
            await SeedTestData();
            SetupUserContext("test@example.com");

            // Act
            var result = await _controller.AccountDetailByAccountNumber("9999999999999999");

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task AccountDetailByAccountNumber_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            var (userAccess, userProfile, accountType, cardType) = await SeedTestData();

            // Create another user
            var otherUserAccess = new TUserAccess
            {
                Id = Guid.NewGuid(),
                EmailPhoneNumber = "other@example.com"
            };
            await _context.TUserAccesses.AddAsync(otherUserAccess);

            // Create wallet account detail
            var walletAccountDetail = new TWalletAccountDetail
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890123456",
                UserProfileId = userProfile.Id,
                UserAccessId = userAccess.Id,
                CardTypeId = cardType.Id,
                AccountTypeId = accountType.Id
            };
            await _context.TWalletAccountDetails.AddAsync(walletAccountDetail);
            await _context.SaveChangesAsync();

            // Set context to different user
            SetupUserContext("other@example.com");

            // Act
            var result = await _controller.AccountDetailByAccountNumber("1234567890123456");

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task AccountDetailByAccountNumber_NoAccountScheme_ReturnsBadRequest()
        {
            // Arrange
            var (userAccess, userProfile, accountType, _) = await SeedTestData();
            SetupUserContext("test@example.com");

            // Create wallet account detail without card type
            var walletAccountDetail = new TWalletAccountDetail
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234567890123456",
                UserProfileId = userProfile.Id,
                UserAccessId = userAccess.Id,
                AccountTypeId = accountType.Id
            };
            await _context.TWalletAccountDetails.AddAsync(walletAccountDetail);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.AccountDetailByAccountNumber("1234567890123456");

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }
    }
}
