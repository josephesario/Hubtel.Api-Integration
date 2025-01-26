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
    /// <summary>
    ///  AccessController
    /// </summary>
    public class AccessController : Controller
    {

        private readonly HubtelWalletDbContext _context;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public AccessController(HubtelWalletDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        /// <summary>
        /// Authenticates a user by their email/phone number and user secret (password).
        /// </summary>
        /// <remarks>
        /// This API endpoint performs user login by accepting an email/phone number and a user secret. 
        /// If the provided credentials are valid, a system access token is returned.
        /// If the credentials are invalid or an error occurs, an appropriate error response will be returned.
        /// </remarks>
        /// <param name="LoginAccess">
        /// The login credentials, including the user's email/phone number and their user secret (password).
        /// </param>
        /// <returns>
        /// Returns an <see cref="ApiResponse{ISystemAccessToken}"/> with a 200 OK status if login is successful,
        /// otherwise returns an <see cref="ApiResponse{string}"/> with a relevant error message.
        /// </returns>
        /// <response code="200">Returns a system access token upon successful login.</response>
        /// <response code="400">Bad request due to invalid input.</response>
        /// <response code="404">User not found.</response>
        /// <response code="409">Conflict, such as incorrect credentials or blocked user.</response>
        /// <response code="500">Internal server error if the login operation fails due to an exception.</response>
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


                var userAccess = await _context.TUserAccesses!.FirstOrDefaultAsync(x => x.EmailPhoneNumber == LoginAccess.EmailPhoneNumber && x.UserSecret == secretEncrypt);

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
                    var tokenResponse = await tokenService.GenerateJwtToken(userAccess,  userAccess.Id.ToString(), userAccess.EmailPhoneNumber);

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



        /// <summary>
        /// Allows a user to change their password.
        /// </summary>
        /// <remarks>
        /// This API endpoint allows an authenticated user to change their password by providing their email/phone number and new password. 
        /// The request must include the user’s current credentials for authentication. The new password will be encrypted before being stored.
        /// If the request is invalid or an error occurs during the process, an appropriate error message is returned.
        /// </remarks>
        /// <param name="changePassword">
        /// The data required to change the password, including the user's email/phone number and new password.
        /// </param>
        /// <returns>
        /// Returns an <see cref="ApiResponse{string}"/> containing a success message if the password is successfully changed, 
        /// otherwise, an error message explaining the failure.
        /// </returns>
        /// <response code="200">Password successfully changed.</response>
        /// <response code="400">Bad request, invalid input (e.g., missing or invalid email/phone number, or password).</response>
        /// <response code="401">Unauthorized, if the user does not match the authenticated user in the request.</response>
        /// <response code="404">User not found, or the provided email/phone number does not match any account.</response>
        /// <response code="500">Internal server error if the password change fails due to an exception.</response>

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
