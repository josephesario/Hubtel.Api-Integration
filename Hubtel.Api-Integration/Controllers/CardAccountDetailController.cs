using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using dbContex.Models;
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
    public class CardAccountDetailController : Controller
    {
        private readonly HubtelWalletDbContextExtended _context;
        private readonly IConfiguration _configuration;

        public CardAccountDetailController(HubtelWalletDbContextExtended context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        #region CreateCardAccountDetail
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ICardAccountDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCardAccountDetail([Required][FromBody] ICardAccountDetail model)
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
                        Success = false
                    });
                }

                var cardType = _context.TCardTypes!.FirstOrDefault(x => x.Name == model.CardType)?.Name;

                if (cardType == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                    {
                        Data = "Card Type does not exist",
                        Message = "Card Type does not exist",
                        Success = false
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

                var existingUserType = await _context.TUserTypes!.FirstOrDefaultAsync(x => x.Id == existingEmailPhone.UserTypeId);


                if (!(existingUserType!.Name!.ToLower().Equals("card")))
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User authenticated",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Errors = new[] { "Sorry Your Current User Type Does Not Support Card Accounts!!" }
                    });
                }


                var cardAccountCount = _context.TCardAccountDetails!.Count(x => x.UserProfileId == legalName.Id);
                var phoneAccountCount = _context.TPhoneAccountDetails!.Count(x => x.UserProfileId == legalName.Id);

                int totalAccountCount = cardAccountCount + phoneAccountCount;

                if (totalAccountCount >= 5)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                    {
                        Data = "You have reached the maximum number of accounts",
                        Message = "You have reached the maximum number of accounts",
                        Success = false
                    });
                }

                var cardNumber = model.CardNumber.Substring(0, 6);

                var cardAccountDetail = new TCardAccountDetail
                {
                    CardNumber = cardNumber,
                    CardTypeId = _context.TCardTypes!.FirstOrDefault(x => x.Name == model.CardType)!.Id,
                    UserProfileId = legalName.Id,
                    UserAccessId = legalName.UserAccessId
                };

                await _context.TCardAccountDetails!.AddAsync(cardAccountDetail);
                var output = await _context.SaveChangesAsync();

                if (output > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, new ApiResponse<ICardAccountDetail>
                    {
                        Data = model,
                        Message = "Card Account Detail created successfully",
                        Success = true
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                    {
                        Data = "An error occurred while creating Card Account Detail",
                        Message = "An error occurred while creating Card Account Detail",
                        Success = false
                    });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Data = ex.Message,
                    Message = "An error occurred while creating Card Account Detail",
                    Success = false
                });
            }
        }
        #endregion

        #region GetCardAccountDetail
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ICardAccountDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCardAccountDetail([Required][FromQuery] string cardNumber)
        {
            try
            {
                var cardAccountDetail = await _context.TCardAccountDetails!.FirstOrDefaultAsync(x => x.CardNumber == cardNumber);
                if (cardAccountDetail == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ApiResponse<string>
                    {
                        Data = "Card Account Detail does not exist",
                        Message = "Card Account Detail does not exist",
                        Success = false
                    });
                }
                var cardType = _context.TCardTypes!.FirstOrDefault(x => x.Id == cardAccountDetail.CardTypeId);
                var cardAccountDetailResponse = new CardAccountDetail
                {
                    CardNumber = cardAccountDetail.CardNumber,
                    CardType = cardType!.Name!,
                    ProfileLegalName = _context.TUserProfiles!.FirstOrDefault(x => x.Id == cardAccountDetail.UserProfileId)!.LegalName
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


                return StatusCode(StatusCodes.Status200OK, new ApiResponse<ICardAccountDetail>
                {
                    Data = cardAccountDetailResponse,
                    Message = "Card Account Detail retrieved successfully",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Data = ex.Message,
                    Message = "An error occurred while retrieving Card Account Detail",
                    Success = false
                });
            }
        }
        #endregion

        #region GetAllCardAccountDetails
        [HttpGet("GetAllCardAccountDetails")]
        [ProducesResponseType(typeof(ApiResponse<List<ICardAccountDetail>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCardAccountDetails()
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

                var userProfile = await _context.TUserProfiles!.FirstOrDefaultAsync(x => x.UserAccessId == existingUser.Id);
                if (userProfile == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                    {
                        Data = "User profile not found",
                        Message = "User profile not found",
                        Success = false
                    });
                }

                var cardAccountDetails = await _context.TCardAccountDetails!
                    .Where(x => x.UserProfileId == userProfile.Id)
                    .Select(card => new CardAccountDetail
                    {
                        CardNumber = card.CardNumber,
                        CardType = _context.TCardTypes!.FirstOrDefault(t => t.Id == card.CardTypeId)!.Name!,
                        ProfileLegalName = userProfile.LegalName
                    })
                    .ToListAsync();



                return StatusCode(StatusCodes.Status200OK, new ApiResponse<List<CardAccountDetail>>
                {
                    Data = cardAccountDetails,
                    Message = "Card Account Details retrieved successfully",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Data = ex.Message,
                    Message = "An error occurred while retrieving Card Account Details",
                    Success = false
                });
            }
        }
        #endregion

        #region DeleteCardAccountDetail
        [HttpDelete("DeleteCardAccountDetail")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCardAccountDetail([Required][FromQuery] string cardNumber)
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

                var cardAccountDetail = await _context.TCardAccountDetails!
                    .FirstOrDefaultAsync(x => x.CardNumber == cardNumber);

                if (cardAccountDetail == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ApiResponse<string>
                    {
                        Data = "Card Account Detail does not exist",
                        Message = "Card Account Detail does not exist",
                        Success = false
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

                _context.TCardAccountDetails!.Remove(cardAccountDetail);
                var output = await _context.SaveChangesAsync();

                if (output > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, new ApiResponse<string>
                    {
                        Data = cardNumber,
                        Message = "Card Account Detail deleted successfully",
                        Success = true
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                    {
                        Data = "An error occurred while deleting Card Account Detail",
                        Message = "An error occurred while deleting Card Account Detail",
                        Success = false
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Data = ex.Message,
                    Message = "An error occurred while deleting Card Account Detail",
                    Success = false
                });
            }
        }
        #endregion

    }
}
