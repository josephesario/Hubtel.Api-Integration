using System.ComponentModel.DataAnnotations;
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
    public class UserProfileController : Controller
    {
        private readonly HubtelWalletDbContext _context;
        private readonly IConfiguration _configuration;

        public UserProfileController(HubtelWalletDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        #region AddProfile
        [Authorize]
        [HttpPost("AddProfile")]
        [ProducesResponseType(typeof(ApiResponse<IUserProfile>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProfile([Required][FromBody] UserProfile userProfile)
        {
            if (userProfile == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid request",
                    Errors = new[] { "Invalid request" }
                });
            }
            if (string.IsNullOrWhiteSpace(userProfile.LegalName) || string.IsNullOrWhiteSpace(userProfile.IdentityCardNumber) || string.IsNullOrWhiteSpace(userProfile.EmailPhone))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid request",
                    Errors = new[] { "LegalName, IdentityCardNumber and PhoneNumber are required" }
                });
            }

            if (!Validations.ValidateGhanaID(userProfile.IdentityCardNumber))
            {
                // Log the problematic ID for investigation
                Console.WriteLine($"Invalid Ghana ID: {userProfile.IdentityCardNumber}");

                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid request",
                    Errors = new[] { "Invalid Ghana ID" }
                });
            }

            if (Validations.IsEmailPhoneValid(userProfile.EmailPhone) == false)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid request",
                    Errors = new[] { "Invalid Phone Number" }
                });
            }

            try
            {
                var currentUserEmailPhone = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


                var existingProfile = await _context.TUserProfiles!.FirstOrDefaultAsync(x => x.IdentityCardNumber == userProfile.IdentityCardNumber);
                if (existingProfile != null)
                {
                    return Conflict(new ApiResponse<string>
                    {
                        Success = false,
                        StatusCode = StatusCodes.Status409Conflict,
                        Message = "Profile already exists",
                        Errors = new[] { "Profile already exists" }
                    });
                }

                var existingEmailPhone = await _context.TUserAccesses!.FirstOrDefaultAsync(x => x.EmailPhoneNumber == currentUserEmailPhone);

                if (existingEmailPhone!.EmailPhoneNumber != userProfile.EmailPhone)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User authenticated",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Errors = new[] { "Make Sure To Provide Your Account Email Or Password" }
                    });

                }



                var profile = new TUserProfile
                {
                    LegalName = userProfile.LegalName,
                    IdentityCardNumber = userProfile.IdentityCardNumber,
                    EmailPhone = userProfile.EmailPhone,
                    CreatedAt = DateTime.Now,
                    UserAccessId = existingEmailPhone!.Id
                };
                await _context.TUserProfiles!.AddAsync(profile);
                await _context.SaveChangesAsync();
                return Ok(new ApiResponse<IUserProfile>
                {
                    Success = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Profile created successfully",
                    Data = userProfile
                });

            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("DbUpdateException caught: " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An unexpected error occurred",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = new[] { ex.InnerException?.Message ?? ex.Message }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("General Exception caught: " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An unexpected error occurred",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = new[] { ex.Message }
                });
            }

        }
        #endregion


        #region GetProfileByEmailGhcPhone
        [Authorize]
        [HttpGet("GetProfileByEmailGhcPhone")]
        [ProducesResponseType(typeof(ApiResponse<IUserProfile>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfileByEmailGhcPhone([Required][FromHeader] string EmailGhcPhone)
        {




            if (EmailGhcPhone == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid request",
                    Errors = new[] { "Invalid request" }
                });
            }
            if (string.IsNullOrWhiteSpace(EmailGhcPhone) || string.IsNullOrWhiteSpace(EmailGhcPhone) || string.IsNullOrWhiteSpace(EmailGhcPhone))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid request",
                    Errors = new[] { "Email, IdentityCardNumber or PhoneNumber  required" }
                });
            }



            if (Validations.ValidateGhanaID(EmailGhcPhone) == false && Validations.IsEmailPhoneValid(EmailGhcPhone) == false)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid request",
                    Errors = new[] { "Invalid Ghana ID , Email Or Phone Number" }
                });
            }





            try
            {

                var userProfile = await _context.TUserProfiles!.FirstOrDefaultAsync(e => e.IdentityCardNumber!.ToLower() == EmailGhcPhone.ToLower() || e.EmailPhone!.ToLower() == EmailGhcPhone.ToLower());
                var currentUserEmailPhone = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

                if(currentUserEmailPhone!= EmailGhcPhone)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User authenticated",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Errors = new[] { "You Are Not Authorize To View This Data" }
                    });
                }

                if (userProfile == null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Profile Not Found",
                        Errors = new[] { "Profile Not Found" }
                    });
                }

                var userAccess = await _context.TUserAccesses!.FirstOrDefaultAsync(e => e.Id == userProfile.UserAccessId);

                if (currentUserEmailPhone != userAccess!.EmailPhoneNumber)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User authenticated",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Errors = new[] { "You Are Not Authorize To View This Data" }
                    });
                }
                else
                {


                    UserProfile profile = new()
                    {
                        IdentityCardNumber = userProfile.IdentityCardNumber,
                        EmailPhone = userProfile.EmailPhone,
                        LegalName = userProfile.LegalName

                    };

                    return Ok(new ApiResponse<IUserProfile>
                    {
                        Success = true,
                        Message = "User data retrieved successfully",
                        StatusCode = StatusCodes.Status200OK,
                        Data = profile
                    });
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("DbUpdateException caught: " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An unexpected error occurred",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = new[] { ex.InnerException?.Message ?? ex.Message }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("General Exception caught: " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An unexpected error occurred",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = new[] { ex.Message }
                });
            }


        }
        #endregion

        #region UpdateProfileByEmailGhcPhone
        [Authorize]
        [HttpPut("UpdateProfileByEmailGhcPhone")]
        [ProducesResponseType(typeof(ApiResponse<IUserProfile>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfileByEmailGhcPhone([Required][FromBody] UserProfile userProfile, [Required][FromHeader] string EmailGhcPhone)
        {

            if (EmailGhcPhone == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid request",
                    Errors = new[] { "Invalid request" }
                });
            }
            if (string.IsNullOrWhiteSpace(EmailGhcPhone) || string.IsNullOrWhiteSpace(EmailGhcPhone) || string.IsNullOrWhiteSpace(EmailGhcPhone))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid request",
                    Errors = new[] { "Email, IdentityCardNumber or PhoneNumber  required" }
                });
            }


            if (Validations.ValidateGhanaID(EmailGhcPhone) == false && Validations.IsEmailPhoneValid(EmailGhcPhone) == false)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid request",
                    Errors = new[] { "Invalid Ghana ID , Email Or Phone Number" }
                });
            }

            try
            {


                var Profile = await _context.TUserProfiles!.FirstOrDefaultAsync(e => e.IdentityCardNumber!.ToLower() == EmailGhcPhone || e.EmailPhone!.ToLower() == EmailGhcPhone);

                var currentUserEmailPhone = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;


                if (userProfile == null || Profile == null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Profile Not Found",
                        Errors = new[] { "Profile Not Found" }
                    });
                }

                var userAccess = await _context.TUserAccesses!.FirstOrDefaultAsync(e => e.Id == Profile!.UserAccessId);

                if (currentUserEmailPhone != userAccess!.EmailPhoneNumber)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User authenticated",
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Errors = new[] { "You Are Not Authorize To View This Data" }
                    });
                }
                else
                {

                    Profile.IdentityCardNumber = userProfile.IdentityCardNumber;
                    Profile.EmailPhone = userProfile.EmailPhone;
                    Profile.LegalName = userProfile.LegalName;

                    _context.TUserProfiles!.Update(Profile);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {

                        return Ok(new ApiResponse<IUserProfile>
                        {
                            Success = true,
                            Message = "User Updated successfully",
                            StatusCode = StatusCodes.Status200OK,
                            Data = userProfile
                        });

                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                        {
                            Success = false,
                            Message = "An unexpected error occurred",
                            StatusCode = StatusCodes.Status500InternalServerError,
                            Errors = new[] { "Profile could not be changed" }
                        });
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("DbUpdateException caught: " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An unexpected error occurred",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = new[] { ex.InnerException?.Message ?? ex.Message }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("General Exception caught: " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An unexpected error occurred",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = new[] { ex.Message }
                });
            }


        }
        #endregion

        #region DeleteProfile
        [Authorize]
        [HttpDelete("DeleteProfile")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProfile([FromQuery] string identityCardNumber)
        {
            if (string.IsNullOrWhiteSpace(identityCardNumber))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid request",
                    Errors = new[] { "Identity Card Number is required" }
                });
            }

            if (Validations.ValidateGhanaID(identityCardNumber) == false)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid request",
                    Errors = new[] { "Invalid Ghana ID" }
                });
            }

            try
            {
                var existingProfile = await _context.TUserProfiles!
                    .FirstOrDefaultAsync(x => x.IdentityCardNumber == identityCardNumber);

                if (existingProfile == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Profile not found",
                        Errors = new[] { "No profile found for the given Identity Card Number" }
                    });
                }

                var existingProfileAccounts = await _context.TWalletAccountDetails!.Where(x => x.UserProfileId == existingProfile.Id).ToListAsync();

                if (existingProfileAccounts.Any())
                {

                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Profile has associated accounts",
                        Errors = new[] { "Profile has associated accounts" }
                    });

                }

                _context.TUserProfiles!.Remove(existingProfile);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Profile deleted successfully"
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An unexpected error occurred",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = new[] { ex.InnerException?.Message ?? ex.Message }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An unexpected error occurred",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = new[] { ex.Message }
                });
            }
        }
        #endregion

    }

}