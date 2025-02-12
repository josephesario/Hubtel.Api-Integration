﻿using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using dbContex.Models;
using Helper.secure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using ViewModel.Data;
using ViewModel.Interfaces;

namespace Hubtel.Api_Integration.Controllers
{
    /// <summary>
    /// Controller for managing user access operations such as registration and retrieval by email or phone.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccessController : Controller
    {

        private readonly HubtelWalletDbContext _context;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAccessController"/> class.
        /// </summary>
        /// <param name="context">Database context for accessing user data.</param>
        /// <param name="configuration">Configuration for retrieving encryption keys.</param>

        public UserAccessController(HubtelWalletDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        #region Register
        /// <summary>
        /// Registers a new user with the provided user access data.
        /// </summary>
        /// <param name="userAccess">The user access data containing email/phone number and password.</param>
        /// <returns>
        /// A status code and message indicating the outcome of the registration.
        /// </returns>
        /// <response code="200">User access successfully created.</response>
        /// <response code="400">Invalid input data provided.</response>
        /// <response code="404">Resource not found.</response>
        /// <response code="409">User access already exists.</response>
        [HttpPost("Register")]
        [ProducesResponseType(typeof(ApiResponse<IUserAccess>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([Required][FromBody] IUserAccess userAccess)
        {
            if (userAccess == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid User Access data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "User Access Data is required" }
                });
            }

            // Validate Email/Phone Number
            if (string.IsNullOrWhiteSpace(userAccess.EmailPhoneNumber) || !Validations.IsEmailPhoneValid(userAccess.EmailPhoneNumber))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid User Access data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "Email Or Phone Number is required" }
                });
            }

            // Validate UserSecret (Password)
            if (string.IsNullOrWhiteSpace(userAccess.UserSecret))
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
                // Rest of the existing implementation remains the same
                string? keyBytesFromJson = _configuration["EncryptionKeys:keyBytes"];
                string? ivBytesFromJson = _configuration["EncryptionKeys:ivBytes"];

                var userAccessExist = await _context.TUserAccesses?.AnyAsync(x => x.EmailPhoneNumber == userAccess.EmailPhoneNumber)!;
                if (userAccessExist)
                {
                    return Conflict(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User Access already exists",
                        StatusCode = StatusCodes.Status409Conflict,
                        Errors = new[] { "User Access already exists" }
                    });
                }

                byte[] keyBytes = Convert.FromBase64String(keyBytesFromJson!);
                byte[] ivBytes = Convert.FromBase64String(ivBytesFromJson!);

                EncryptionService encryptionService = new(keyBytes, ivBytes);
                string secretEncrypt = encryptionService.Encrypt(userAccess.UserSecret!);

                var userAccessEntity = new TUserAccess
                {
                    EmailPhoneNumber = userAccess.EmailPhoneNumber,
                    UserSecret = secretEncrypt,
                };

                await _context.TUserAccesses!.AddAsync(userAccessEntity);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    UserAccess userAcc = new UserAccess
                    {
                        EmailPhoneNumber = userAccessEntity.EmailPhoneNumber,
                        UserSecret = userAccess.UserSecret,
                    };

                    return Ok(new ApiResponse<UserAccess>
                    {
                        Success = true,
                        Message = "User Access created successfully",
                        StatusCode = StatusCodes.Status200OK,
                        Data = userAcc
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                    {
                        Success = false,
                        Message = "An unexpected error occurred",
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Errors = new[] { "User Access could not be created" }
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


        #region GetUserByEmailPhone
        /// <summary>
        /// Retrieves a user's details by their email or phone number.
        /// </summary>
        /// <param name="emailPhone">The email or phone number of the user.</param>
        /// <returns>
        /// A status code and the user's access details if found.
        /// </returns>
        /// <response code="200">User access successfully retrieved.</response>
        /// <response code="404">User access not found.</response>
        /// <response code="401">Unauthorized access to the requested data.</response>
        [Authorize]
        [HttpGet("GetUserByEmailPhone")]
        [ProducesResponseType(typeof(ApiResponse<IUserAccess>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserByEmailPhone([Required][FromHeader] string emailPhone)
        {

            var currentUserEmailPhone = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            if (currentUserEmailPhone != emailPhone)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User authenticated",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Errors = new[] { "You Are Not Authorize To View This Data" }
                });
            }

            if (string.IsNullOrEmpty(currentUserEmailPhone))
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not authenticated",
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Errors = new[] { "No authentication token provided" }
                });
            }

            var userAccess = await _context.TUserAccesses!
                .FirstOrDefaultAsync(x => x.EmailPhoneNumber == currentUserEmailPhone);

            if (userAccess == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found",
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = new[] { "No user found for this account" }
                });
            }

            UserAccess userAcc = new UserAccess
            {
                EmailPhoneNumber = userAccess.EmailPhoneNumber,
                UserSecret = userAccess.UserSecret,
            };

            return Ok(new ApiResponse<UserAccess>
            {
                Success = true,
                Message = "User data retrieved successfully",
                StatusCode = StatusCodes.Status200OK,
                Data = userAcc
            });
        }
        #endregion



        #region Delete
        /// <summary>
        /// Deletes a user access based on the provided email or phone number.
        /// This operation removes the user's access from the system if no dependent data exists.
        /// </summary>
        /// <param name="emailPhoneNumber">The email or phone number of the user whose access is to be deleted. This parameter is required and must be valid.</param>
        /// <returns>A response indicating the success or failure of the operation.</returns>
        /// <response code="200">The user access was successfully deleted.</response>
        /// <response code="400">The provided email or phone number is invalid or missing.</response>
        /// <response code="404">No user access was found with the provided email or phone number.</response>
        /// <response code="409">The user access cannot be deleted because there are dependent data (e.g., associated wallet account).</response>
        /// <response code="500">An unexpected error occurred while processing the request.</response>
        /// <response code="503">The service is unavailable due to a temporary issue (e.g., database unavailability).</response>

        [HttpDelete("Delete")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUserAccess([Required][FromHeader] string emailPhoneNumber)
        {

            if (string.IsNullOrWhiteSpace(emailPhoneNumber) || !Validations.IsEmailPhoneValid(emailPhoneNumber))
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

                var userAccess = await _context.TUserAccesses!.FirstOrDefaultAsync(x => x.EmailPhoneNumber == emailPhoneNumber);
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


                var CardAccount = await _context.TWalletAccountDetails!.FirstOrDefaultAsync(x => x.UserAccessId == userAccess.Id);

                if (CardAccount!=null)
                {
                    return Conflict(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User Access has dependent data",
                        StatusCode = StatusCodes.Status409Conflict,
                        Errors = new[] { "User Access has dependent data" }
                    });
                }
                else { 


                    _context.TUserAccesses!.Remove(userAccess);
                    var result = await _context.SaveChangesAsync();
                    if (result > 0)
                    {
                        return Ok(new ApiResponse<string>
                        {
                            Success = true,
                            Message = "User Access deleted successfully",
                            StatusCode = StatusCodes.Status200OK,
                            Data = $"User With Email Or Password {userAccess.EmailPhoneNumber} Deleted Successfully!"

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

    }

}
