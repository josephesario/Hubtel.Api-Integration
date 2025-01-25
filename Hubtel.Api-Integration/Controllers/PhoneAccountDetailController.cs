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
    public class PhoneAccountDetailController : Controller
    {
        private readonly HubtelWalletDbContextExtended _context;
        private readonly IConfiguration _configuration;

        public PhoneAccountDetailController(HubtelWalletDbContextExtended context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        #region CreatePhoneAccountDetail
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<IPhoneAccountDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePhoneAccountDetail([Required][FromBody] IPhoneAccountDetail model)
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

                var simCardType = _context.TSimcardTypes!.FirstOrDefault(x => x.Name == model.SimCardType)?.Name;

                if (simCardType == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                    {
                        Data = "Sim Card Type does not exist",
                        Message = "Sim Card Type does not exist",
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
                        Errors = new[] { "Sorry Your Current User Type Does Not Support Phone Accounts!!" }
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

                var phoneAccountDetail = new TPhoneAccountDetail
                {
                    PhoneNumber = model.PhoneNumber,
                    SimCardTypeId = _context.TSimcardTypes!.FirstOrDefault(x => x.Name == model.SimCardType)!.Id,
                    UserProfileId = legalName.Id,
                    UserAccessId = legalName.UserAccessId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.TPhoneAccountDetails!.AddAsync(phoneAccountDetail);
                var output = await _context.SaveChangesAsync();

                if (output > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, new ApiResponse<IPhoneAccountDetail>
                    {
                        Data = model,
                        Message = "Phone Account Detail created successfully",
                        Success = true
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                    {
                        Data = "An error occurred while creating Phone Account Detail",
                        Message = "An error occurred while creating Phone Account Detail",
                        Success = false
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Data = ex.Message,
                    Message = "An error occurred while creating Phone Account Detail",
                    Success = false
                });
            }
        }
        #endregion

        #region GetPhoneAccountDetail
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IPhoneAccountDetailOut>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPhoneAccountDetail([Required][FromQuery] string phoneNumber)
        {
            try
            {
                var phoneAccountDetail = await _context.TPhoneAccountDetails!.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
                if (phoneAccountDetail == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ApiResponse<string>
                    {
                        Data = "Phone Account Detail does not exist",
                        Message = "Phone Account Detail does not exist",
                        Success = false
                    });
                }

                var simCardType = _context.TSimcardTypes!.FirstOrDefault(x => x.Id == phoneAccountDetail.SimCardTypeId);
                var phoneAccountDetailResponse = new PhoneAccountDetailOut
                {
                    PhoneNumber = phoneAccountDetail.PhoneNumber,
                    SimCardType = simCardType!.Name!,
                    ProfileLegalName = _context.TUserProfiles!.FirstOrDefault(x => x.Id == phoneAccountDetail.UserProfileId)!.LegalName,
                    CreatedAt = phoneAccountDetail.CreatedAt
                };

                var currentUserEmailPhone = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

                var existingEmailPhone = await _context.TUserAccesses!.FirstOrDefaultAsync(x => x.EmailPhoneNumber == currentUserEmailPhone);
                var existingEmail = await _context.TUserAccesses!.FirstOrDefaultAsync(x => x.Id == phoneAccountDetail.UserAccessId);

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

                return StatusCode(StatusCodes.Status200OK, new ApiResponse<IPhoneAccountDetailOut>
                {
                    Data = phoneAccountDetailResponse,
                    Message = "Phone Account Detail retrieved successfully",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Data = ex.Message,
                    Message = "An error occurred while retrieving Phone Account Detail",
                    Success = false
                });
            }
        }
        #endregion

        #region GetAllPhoneAccountDetails
        [HttpGet("GetAllPhoneAccountDetails")]
        [ProducesResponseType(typeof(ApiResponse<List<IPhoneAccountDetailOut>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPhoneAccountDetails()
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

                var phoneAccountDetails = await _context.TPhoneAccountDetails!
                    .Where(x => x.UserProfileId == userProfile.Id)
                    .Select(phone => new PhoneAccountDetailOut
                    {
                        PhoneNumber = phone.PhoneNumber,
                        SimCardType = _context.TSimcardTypes!.FirstOrDefault(t => t.Id == phone.SimCardTypeId)!.Name!,
                        ProfileLegalName = userProfile.LegalName,
                        CreatedAt = phone.CreatedAt
                    })
                    .ToListAsync();

                return StatusCode(StatusCodes.Status200OK, new ApiResponse<List<PhoneAccountDetailOut>>
                {
                    Data = phoneAccountDetails,
                    Message = "Phone Account Details retrieved successfully",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Data = ex.Message,
                    Message = "An error occurred while retrieving Phone Account Details",
                    Success = false
                });
            }
        }
        #endregion

        #region DeletePhoneAccountDetail
        [HttpDelete("DeletePhoneAccountDetail")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePhoneAccountDetail([Required][FromQuery] string phoneNumber)
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

                var phoneAccountDetail = await _context.TPhoneAccountDetails!
                    .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);

                if (phoneAccountDetail == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ApiResponse<string>
                    {
                        Data = "Phone Account Detail does not exist",
                        Message = "Phone Account Detail does not exist",
                        Success = false
                    });
                }

                var userProfile = await _context.TUserProfiles!
                    .FirstOrDefaultAsync(x => x.Id == phoneAccountDetail.UserProfileId);

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

                _context.TPhoneAccountDetails!.Remove(phoneAccountDetail);
                var output = await _context.SaveChangesAsync();

                if (output > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, new ApiResponse<string>
                    {
                        Data = phoneNumber,
                        Message = "Phone Account Detail deleted successfully",
                        Success = true
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                    {
                        Data = "An error occurred while deleting Phone Account Detail",
                        Message = "An error occurred while deleting Phone Account Detail",
                        Success = false
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Data = ex.Message,
                    Message = "An error occurred while deleting Phone Account Detail",
                    Success = false
                });
            }
        }
        #endregion
    }
}