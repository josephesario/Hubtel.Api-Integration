﻿using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using dbContex.Models;
using Helper.secure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModel.Data;
using ViewModel.Interfaces;

namespace Hubtel.Api_Integration.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class WalletAccountDetailController : Controller
    {
        private readonly HubtelWalletDbContext _context;
        private readonly IConfiguration _configuration;

        public WalletAccountDetailController(HubtelWalletDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        #region CreateAccountDetail
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<IWalletAccountDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAccountDetail([Required][FromHeader] string AccountType, [Required][FromBody] IWalletAccountDetail model)
        {
            try
            {

                var legalName = await _context.TUserProfiles!.FirstOrDefaultAsync(x => x.LegalName == model.ProfileLegalName);
                if (legalName == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                    {
                        Data = "Profile does not exist",
                        Message = "Profile does not exist",
                        Success = false,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }


                if (string.IsNullOrWhiteSpace(model.AccountNumber) || string.IsNullOrWhiteSpace(AccountType) || string.IsNullOrWhiteSpace(model.AccountScheme))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                    {
                        Data = "Account Number, Account Type and Card Type are required",
                        Message = "Account Number, Account Type and Card Type are required",
                        Success = false,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                if (!Validations.IsEmailPhoneValid(model.AccountNumber))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                    {
                        Data = "Account Number is invalid",
                        Message = "Account Number Should Be either a phone number or a card",
                        Success = false,
                        StatusCode = StatusCodes.Status400BadRequest
                    });

                }


                var simType = _context.TSimcardTypes!.FirstOrDefault(x => x.Name == model.AccountScheme)?.Name;
                var cardType = _context.TCardTypes!.FirstOrDefault(x => x.Name == model.AccountScheme)?.Name;

                if (simType == null && cardType == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                    {
                        Data = "Card Type Or Provided Sim Card type does not exist",
                        Message = "Card Type Or Provided Sim Card type does not exist",
                        Success = false
                    });
                }



                var validateCardNumber = await _context.TWalletAccountDetails!.FirstOrDefaultAsync(x => x.AccountNumber == model.AccountNumber);
                if (validateCardNumber != null)
                {
                    return Conflict(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Wallet Account Already exist",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Errors = new[] { "Wallet Already exist" }
                    });
                }


                var currentUserEmailPhone = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


                var existingEmailPhone = await _context.TUserAccesses!.FirstOrDefaultAsync(x => x.EmailPhoneNumber == currentUserEmailPhone);

                if (existingEmailPhone!.EmailPhoneNumber != legalName.EmailPhone)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User authenticated",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Errors = new[] { "You Are Not Authorize To Perform This Operation" }
                    });

                }


                var cardAccountCount = _context.TWalletAccountDetails!.Count(x => x.UserProfileId == legalName.Id);


                if (cardAccountCount >= 5)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                    {
                        Data = "You have reached the maximum number of (5) accounts",
                        Message = "Maximum number of (5) accounts Reached",
                        Success = false,
                        StatusCode = StatusCodes.Status400BadRequest

                    });
                }


                var accountTypeExist = _context.TTypes!.FirstOrDefault(x => x.Name == AccountType);
                if (accountTypeExist == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ApiResponse<string>
                    {
                        Data = "Account Type does not exist",
                        Message = "Account Type does not exist",
                        Success = false,
                        StatusCode = StatusCodes.Status404NotFound
                    });
                }

                string cardNumber = string.Empty;
                string squemaCardId = string.Empty;
                string squemaSimId = string.Empty;

                if (accountTypeExist.Name!.ToLower().Equals("card")) {

                    if (model.AccountNumber.Length == 16)
                    {
                        if (cardType != null) 
                        { 
                            cardNumber = model.AccountNumber!.Substring(0, 6);
                            squemaCardId = _context.TCardTypes!.FirstOrDefault(x => x.Name == model.AccountScheme)!.Id.ToString();
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                            {
                                Data = "Make Sure The Squema You Have Selected Belongs To The Account Type!!",
                                Message = "Make Sure The Squema You Have Selected Belongs To The Account Type!!",
                                Success = false
                            });
                        }

                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                        {
                            Data = "Make Sure To Enter A valid Card Number 16 Degits for (Visa Or Master Card!)!!",
                            Message = "Make Sure To Enter A valid Card Number 16 Degits!!",
                            Success = false,
                            StatusCode = 400
                            
                        });
                    }

                }
                else
                {
                    var outPhoneNumber = Validations.IsPhoneNumberValid(model.AccountNumber);
                    if (outPhoneNumber)
                    {
                        if (accountTypeExist.Name!.ToLower().Equals("momo"))
                        {
                            if (simType != null)
                            {
                                cardNumber = model.AccountNumber;
                                var outputResult = _context.TSimcardTypes!.FirstOrDefault(x => x.Name == model.AccountScheme)!.Id.ToString();
                                if (outputResult==null)
                                {
                                    squemaSimId = null;
                                }
                                else
                                {
                                    squemaSimId = outputResult;
                                }

                            }
                            else
                            {
                                return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                                {
                                    Data = "Make Sure The Squema You Have Selected Belongs To The Account Type!!",
                                    Message = "Make Sure The Squema You Have Selected Belongs To The Account Type!!",
                                    Success = false,
                                    StatusCode = 400
                                });
                            }
                            

                        }
                       
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                        {
                            Data = "Invalide Phone Number Or Card Provided",
                            Message = "Invalide Phone Number Or Card Provided",
                            Success = false,
                            StatusCode = 400
                        });
                    }
                }




                var cardAccountDetail = new TWalletAccountDetail()
                {
                    AccountNumber = model.AccountNumber,
                    CardTypeId = !string.IsNullOrEmpty(squemaCardId) ? Guid.Parse(squemaCardId) : null,
                    SimCardTypeId = !string.IsNullOrEmpty(squemaSimId) ? Guid.Parse(squemaSimId) : null,
                    UserProfileId = legalName.Id,
                    UserAccessId = legalName.UserAccessId,
                    CreatedAt = DateTime.Now,
                    AccountTypeId = accountTypeExist.Id
                };


                await _context.TWalletAccountDetails!.AddAsync(cardAccountDetail);
                var output = await _context.SaveChangesAsync();

                if (output > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, new ApiResponse<IWalletAccountDetail>
                    {
                        Data = model,
                        Message = "Wallet Created successfully",
                        Success = true,
                        StatusCode = 200

                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                    {
                        Data = "An error occurred while creating Your Wallet Account Detail",
                        Message = "An error occurred while creating Your Wallet Account Detail",
                        Success = false,
                        StatusCode = 500

                    });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Data = ex.Message,
                    Message = "An error occurred while creating Your Wallet Account Detail",
                    Success = false
                });
            }
        }
        #endregion

        #region AccountDetailByAccountNumber
        [HttpGet("AccountDetailByAccountNumber")]
        [ProducesResponseType(typeof(ApiResponse<IWalletAccountDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AccountDetailByAccountNumber([Required][FromQuery] string AccountNumber)
        {
            try
            {
                var cardAccountDetail = await _context.TWalletAccountDetails!.FirstOrDefaultAsync(x => x.AccountNumber == AccountNumber);
                if (cardAccountDetail == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ApiResponse<string>
                    {
                        Data = "Account Not Found",
                        Message = "Account Not Found",
                        Success = false,
                        StatusCode = StatusCodes.Status404NotFound
                    });
                }

                var accountScheme = _context.TCardTypes?.FirstOrDefault(x => x.Id == cardAccountDetail.CardTypeId)?.Name
                                    ?? _context.TSimcardTypes?.FirstOrDefault(x => x.Id == cardAccountDetail.SimCardTypeId)?.Name;

                if (string.IsNullOrEmpty(accountScheme))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Account Does Not Found",
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = new[] { "No matching Account Number could be found for the given account" }
                    });
                }

                var cardAccountDetailResponse = new WalletAccountDetail
                {
                    AccountNumber = cardAccountDetail.AccountNumber,
                    AccountScheme = accountScheme,
                    ProfileLegalName = _context.TUserProfiles?.FirstOrDefault(x => x.Id == cardAccountDetail.UserProfileId)?.LegalName!
                };

                var currentUserEmailPhone = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
                var existingEmailPhone = await _context.TUserAccesses!.FirstOrDefaultAsync(x => x.EmailPhoneNumber == currentUserEmailPhone);
                var existingEmail = await _context.TUserAccesses!.FirstOrDefaultAsync(x => x.Id == cardAccountDetail.UserAccessId);

                if (existingEmailPhone!.EmailPhoneNumber != existingEmail!.EmailPhoneNumber)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User authenticated",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Errors = new[] { "You Are Not Authorize To Perform This Operation" }
                    });
                }

                return StatusCode(StatusCodes.Status200OK, new ApiResponse<IWalletAccountDetail>
                {
                    Data = cardAccountDetailResponse,
                    Message = "Account Detail retrieved successfully",
                    Success = true,
                    StatusCode = StatusCodes.Status200OK
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Data = ex.Message,
                    Message = "An error occurred while retrieving Card Account Detail",
                    Success = false,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
            }
        }
        #endregion

        #region GetAllCardAccountDetails
        [HttpGet("GetAllCardAccountDetails")]
        [ProducesResponseType(typeof(ApiResponse<List<IWalletAccountDetail>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAccountDetails()
        {
            try
            {
                var currentUserEmailPhone = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrWhiteSpace(currentUserEmailPhone))
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User not authenticated",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Errors = new[] { "You are not authorized to perform this operation" }
                    });
                }

                var existingUser = await _context.TUserAccesses
                    ?.Where(x => x.EmailPhoneNumber == currentUserEmailPhone)
                    .FirstOrDefaultAsync()!;

                if (existingUser == null)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User not authenticated",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Errors = new[] { "You are not authorized to perform this operation" }
                    });
                }

                var userProfile = await _context.TUserProfiles!.FirstOrDefaultAsync(x => x.UserAccessId == existingUser.Id);
                if (userProfile == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                    {
                        Data = "User profile not found",
                        Message = "User profile not found",
                        Success = false,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }


                var cardAccountDetails = await _context.TWalletAccountDetails!
                    .Where(x => x.UserProfileId == userProfile.Id)
                    .Select(card => new WalletAccountDetailOut
                    {
                        AccountNumber = card.AccountNumber,
                        AccountScheme = (_context.TCardTypes!.FirstOrDefault(t => t.Id == card.CardTypeId)!.Name!) ?? (_context.TSimcardTypes!.FirstOrDefault(t => t.Id == card.SimCardTypeId)!.Name!),
                        ProfileLegalName = userProfile.LegalName,
                        AccountType = _context.TTypes!.FirstOrDefault(t => t.Id == card.AccountTypeId)!.Name!,
                        CreatedAt = card.CreatedAt!.Value,

                    })
                    .ToListAsync();



                return StatusCode(StatusCodes.Status200OK, new ApiResponse<List<WalletAccountDetailOut>>
                {
                    Data = cardAccountDetails,
                    Message = "Card Account Details retrieved successfully",
                    Success = true,
                    StatusCode = StatusCodes.Status200OK
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Data = ex.Message,
                    Message = "An error occurred while retrieving Card Account Details",
                    Success = false,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
            }
        }
        #endregion

        #region DeleteAccountDetailByAccountNumber
        [HttpDelete("DeleteAccountDetailByAccountNumber")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAccountDetailByAccountNumber([Required][FromQuery] string AccountNumber)
        {
            try
            {
                var currentUserEmailPhone = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
                var existingUser = await _context.TUserAccesses!.FirstOrDefaultAsync(x => x.EmailPhoneNumber == currentUserEmailPhone);

                if (existingUser == null)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User not authenticated",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Errors = new[] { "You are not authorized to perform this operation" }
                    });
                }

                var cardAccountDetail = await _context.TWalletAccountDetails!
                    .FirstOrDefaultAsync(x => x.AccountNumber == AccountNumber);

                if (cardAccountDetail == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ApiResponse<string>
                    {
                        Data = "Account Number does not exist",
                        Message = "Account Number does not exist",
                        Success = false,
                        StatusCode = StatusCodes.Status404NotFound
                    });
                }

                var userProfile = await _context.TUserProfiles!
                    .FirstOrDefaultAsync(x => x.Id == cardAccountDetail.UserProfileId);

                if (userProfile == null || userProfile.UserAccessId != existingUser.Id)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User authenticated",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Errors = new[] { "You Are Not Authorized To Perform This Operation" }
                    });
                }

                _context.TWalletAccountDetails!.Remove(cardAccountDetail);
                var output = await _context.SaveChangesAsync();

                return output > 0
                    ? StatusCode(StatusCodes.Status200OK, new ApiResponse<string>
                    {
                        Data = AccountNumber,
                        Message = "Account Number deleted successfully",
                        Success = true,
                        StatusCode = StatusCodes.Status200OK
                    })
                    : StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                    {
                        Data = "An error occurred while deleting Card Account Detail",
                        Message = "An error occurred while deleting Card Account Detail",
                        Success = false,
                        StatusCode = StatusCodes.Status500InternalServerError
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Data = ex.Message,
                    Message = "An error occurred while deleting Card Account Detail",
                    Success = false,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
            }
        }
        #endregion

    }
}
