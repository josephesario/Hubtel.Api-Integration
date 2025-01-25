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
    public class AccessController : Controller
    {

        private readonly HubtelWalletDbContextExtended _context;
        private readonly IConfiguration _configuration;

        public AccessController(HubtelWalletDbContextExtended context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        #region Login
        [HttpPost("Login")]
        [ProducesResponseType(typeof(ApiResponse<ISystemAccessToken>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([Required][FromBody] ILogin LoginAccess)
        {
            if (LoginAccess == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid Credentials",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "Invalid Credentials" }
                });
            }
            if (string.IsNullOrWhiteSpace(LoginAccess.EmailPhoneNumber) || !Validations.IsEmailPhoneValid(LoginAccess.EmailPhoneNumber))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid User Access data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "Email Or Phone Number is required" }
                });
            }
            if (string.IsNullOrWhiteSpace(LoginAccess.UserSecret))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid User Access data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "Password is required" }
                });
            }
            try
            {

                string? keyBytesFromJson = _configuration["EncryptionKeys:keyBytes"];
                string? ivBytesFromJson = _configuration["EncryptionKeys:ivBytes"];

                byte[] keyBytes = Convert.FromBase64String(keyBytesFromJson!);
                byte[] ivBytes = Convert.FromBase64String(ivBytesFromJson!);

                EncryptionService encryptionService = new(keyBytes, ivBytes);
                string secretEncrypt = encryptionService.Encrypt(LoginAccess.UserSecret);


                var userAccess = await _context.TUserAccesses!
                    .Include(x => x.UserType)
                    .FirstOrDefaultAsync(x => x.EmailPhoneNumber == LoginAccess.EmailPhoneNumber && x.UserSecret == secretEncrypt);

                if (userAccess == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Login Failed",
                        StatusCode = StatusCodes.Status404NotFound,
                        Errors = new[] { "Wrong Credentials Provided" }
                    });

                }
                else if (userAccess != null)
                {

                    Token tokenService = new(_configuration, _context);
                    var tokenResponse = await tokenService.GenerateJwtToken(userAccess, userAccess.UserTypeId, userAccess.Id.ToString(), userAccess.EmailPhoneNumber);

                    if (tokenResponse == null)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                        {
                            Success = false,
                            Message = "An unexpected error occurred",
                            StatusCode = StatusCodes.Status500InternalServerError,
                            Errors = new[] { "Login Failed" }
                        });
                    }

                    SystemAccessToken systemAccess = new()
                    {
                        AccessToken = tokenResponse["accessToken"],
                    };

                    return Ok(new ApiResponse<SystemAccessToken>
                    {
                        Success = true,
                        Message = "Login Successful",
                        StatusCode = StatusCodes.Status200OK,
                        Data = systemAccess
                    });

                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                    {
                        Success = false,
                        Message = "An unexpected error occurred",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Errors = new[] { "Login Failed" }
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



        #region SwitchUserType
        [HttpPatch("SwitchUserType")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SwitchUserType([Required][FromHeader] string UserType)
        {

            if (string.IsNullOrWhiteSpace(UserType) || !Validations.IsEmailPhoneValid(UserType))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid User Access data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "Email Or Phone Number is required" }
                });
            }
            try
            {

                var allowedUserTypes = new[] { "momo", "card" };
                if (!allowedUserTypes.Contains(UserType.ToLower()))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid user type",
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = new[] { "User type must be either 'momo' or 'card'" }
                    });
                }

                var currentUserEmailPhone = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;



                var userAccess = await _context.TUserAccesses!.FirstOrDefaultAsync(x => x.EmailPhoneNumber == currentUserEmailPhone);
                if (userAccess == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User Access not found",
                        StatusCode = StatusCodes.Status404NotFound,
                        Errors = new[] { "User Access not found" }
                    });
                }


                var CardAccount = await _context.TCardAccountDetails!.FirstOrDefaultAsync(x => x.UserAccessId == userAccess.Id);
                var PhoneAccount = await _context.TPhoneAccountDetails!.FirstOrDefaultAsync(x => x.UserAccessId == userAccess.Id);

                if (CardAccount != null || PhoneAccount != null)
                {
                    return Conflict(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User Access has dependent data",
                        StatusCode = StatusCodes.Status409Conflict,
                        Errors = new[] { "User Access has dependent data" }
                    });
                }
                else
                {


                    var userType = await _context.TUserTypes!.FirstOrDefaultAsync(x => x.Name!.ToLower() == UserType.ToLower());

                    if (userType == null)
                    {
                        return NotFound(new ApiResponse<string>
                        {
                            Success = false,
                            Message = "User Type not found",
                            StatusCode = StatusCodes.Status404NotFound,
                            Errors = new[] { "User Type not found" }
                        });
                    }



                    userAccess.UserTypeId = userType.Id;
                    _context.TUserAccesses!.Update(userAccess);
                    var result = await _context.SaveChangesAsync();


                    if (result > 0)
                    {
                        return Ok(new ApiResponse<string>
                        {
                            Success = true,
                            Message = "User Type Updated successfully",
                            StatusCode = StatusCodes.Status200OK,
                            Data = $"User Type Updated successfully!"

                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                        {
                            Success = false,
                            Message = "An unexpected error occurred",
                            StatusCode = StatusCodes.Status500InternalServerError,
                            Errors = new[] { "User Access could not be deleted" }
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

        #region ChangePassword
        [Authorize]
        [HttpPatch("ChangePassword")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([Required][FromBody] ILogin changePassword)
        {

            var currentUserEmailPhone = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            if (currentUserEmailPhone != changePassword.EmailPhoneNumber)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User authenticated",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Errors = new[] { "You Are Not Authorize To View This Data" }
                });
            }


            if (changePassword == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid Change Password data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "Change Password Data is required" }
                });
            }
            if (string.IsNullOrWhiteSpace(changePassword.EmailPhoneNumber) || !Validations.IsEmailPhoneValid(changePassword.EmailPhoneNumber))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid Change Password data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "Email Or Phone Number is required" }
                });
            }

            if (string.IsNullOrWhiteSpace(changePassword.UserSecret))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid Change Password data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "New Password is required" }
                });
            }
            try
            {
                string? keyBytesFromJson = _configuration["EncryptionKeys:keyBytes"];
                string? ivBytesFromJson = _configuration["EncryptionKeys:ivBytes"];

                byte[] keyBytes = Convert.FromBase64String(keyBytesFromJson!);
                byte[] ivBytes = Convert.FromBase64String(ivBytesFromJson!);

                EncryptionService encryptionService = new(keyBytes, ivBytes);
                string newPasswordEncrypt = encryptionService.Encrypt(changePassword.UserSecret);

                var userAccess = await _context.TUserAccesses!
                    .FirstOrDefaultAsync(x => x.EmailPhoneNumber == changePassword.EmailPhoneNumber);

                if (userAccess == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Change Password Failed",
                        StatusCode = StatusCodes.Status404NotFound,
                        Errors = new[] { "Wrong Credentials Provided" }
                    });
                }
                else if (userAccess != null)
                {


                    userAccess.UserSecret = newPasswordEncrypt;
                    _context.TUserAccesses!.Update(userAccess);
                    var result = await _context.SaveChangesAsync();

                    if (result > 0)
                    {
                        return Ok(new ApiResponse<string>
                        {
                            Success = true,
                            Message = "Password changed successfully",
                            StatusCode = StatusCodes.Status200OK,
                            Data = "Password Changed Successfully!"
                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                        {
                            Success = false,
                            Message = "An unexpected error occurred",
                            StatusCode = StatusCodes.Status500InternalServerError,
                            Errors = new[] { "Password could not be changed" }
                        });
                    }

                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                    {
                        Success = false,
                        Message = "An unexpected error occurred",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Errors = new[] { "Password could not be changed" }
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
    }

}
