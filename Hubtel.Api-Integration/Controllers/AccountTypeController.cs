using System;
using System.ComponentModel.DataAnnotations;
using dbContex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModel.Data;
using ViewModel.Interfaces;

namespace Hubtel.Api_Integration.Controllers
{

    /// <summary>
    /// Handles API requests related to account types (user types) such as creating, retrieving, updating, and deleting account types.
    /// </summary>
    /// <remarks>
    /// This controller allows administrators to manage account types such as "momo" and "card". It supports CRUD operations for account types.
    /// </remarks>
    #region AccountTypeController
    [ApiController]
    [Route("api/[controller]")]
    public class AccountTypeController : ControllerBase
    {

        private readonly HubtelWalletDbContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public AccountTypeController(HubtelWalletDbContext context)
        {
            _context = context;
        }

        #region AddUserType
        /// <summary>
        /// Adds a new account type (momo or card).
        /// </summary>
        /// <remarks>
        /// This endpoint adds a new user type to the system. The user type name must be either "momo" or "card". 
        /// If the user type already exists, a conflict is returned. Otherwise, the new user type is added to the system.
        /// </remarks>
        /// <param name="accountType">The account type (user type) to add, by default (momo and card).</param>
        /// <returns>An API response indicating the success or failure of the operation.</returns>
        /// <response code="200">User type successfully added.</response>
        /// <response code="400">Invalid user type data provided.</response>
        /// <response code="409">A user type with the same name already exists.</response>
        /// <response code="500">An error occurred while processing the request.</response>
        [HttpPost("AddAccountType")]
        [ProducesResponseType(typeof(ApiResponse<IAccountType>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AccountType([Required][FromBody] IAccountType accountType)
        {
            try
            {
                if (accountType == null || string.IsNullOrWhiteSpace(accountType.Name))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid user type data",
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = new[] { "User type name is required" }
                    });
                }

                // Validate that the user type is either "momo" or "card"
                var allowedUserTypes = new[] { "momo", "card" };
                if (!allowedUserTypes.Contains(accountType.Name.ToLower()))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid user type",
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = new[] { "User type must be either 'momo' or 'card'" }
                    });
                }

                var userTypeExists = await _context.TTypes!
                    .AnyAsync(e => e.Name!.ToLower() == accountType.Name!.ToLower(), default);

                if (userTypeExists)
                {
                    return Conflict(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User type already exists",
                        StatusCode = StatusCodes.Status409Conflict,
                        Errors = new[] { $"A user type with name '{accountType.Name}' already exists" }
                    });
                }

                var userTypeEntity = new TType
                {
                    Name = accountType.Name,
                    CreatedAt = DateTime.UtcNow,
                };

                await _context.TTypes!.AddAsync(userTypeEntity);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<TType>
                {
                    Success = true,
                    Message = "User Type Added Successfully",
                    StatusCode = StatusCodes.Status200OK,
                    Data = userTypeEntity
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

        #region GetUserType
        /// <summary>
        /// Retrieves a specific account type by its name.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves an account type based on its name (either "momo" or "card"). If the account type does not exist, a 404 response is returned.
        /// </remarks>
        /// <param name="Name">The name of the account type to retrieve.</param>
        /// <returns>The details of the requested account type.</returns>
        /// <response code="200">The requested account type was found.</response>
        /// <response code="400">Invalid user type name provided.</response>
        /// <response code="404">The requested account type was not found.</response>
        /// <response code="500">An error occurred while processing the request.</response>
        [HttpGet("GetAccountTypeByName")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAccountTypeByName([FromHeader][Required] string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid user type data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "User type name is required" }
                });
            }

            Name = Name.ToLower();
            if (Name != "momo" && Name != "card")
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid user type",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "User type must be either 'momo' or 'card'" }
                });
            }

            try
            {
                var userType = await _context.TTypes!
                    .FirstOrDefaultAsync(e => e.Name!.ToLower() == Name);

                return userType == null
                    ? NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User type not found",
                        StatusCode = StatusCodes.Status404NotFound,
                        Errors = new[] { $"User type with name '{Name}' not found" }
                    })
                    : Ok(new ApiResponse<TType>
                    {
                        Success = true,
                        Message = "User Type Found",
                        StatusCode = StatusCodes.Status200OK,
                        Data = userType
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

        #region GetAllUserTypes
        /// <summary>
        /// Retrieves all account types.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of all available account types. If no account types exist, a 404 response is returned.
        /// </remarks>
        /// <returns>A list of all account types.</returns>
        /// <response code="200">All account types were found.</response>
        /// <response code="404">No account types were found.</response>
        /// <response code="500">An error occurred while processing the request.</response>
        [HttpGet("GetAllAccountType")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TType>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAccountType()
        {
            try
            {
                var userTypes = await _context.TTypes!.ToListAsync();
                return userTypes.Count == 0
                    ? NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "No user types found",
                        StatusCode = StatusCodes.Status404NotFound,
                        Errors = new[] { "No user types found" }
                    })
                    : Ok(new ApiResponse<IEnumerable<TType>>
                    {
                        Success = true,
                        Message = "User Types Found",
                        StatusCode = StatusCodes.Status200OK,
                        Data = userTypes
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


        #region DeleteUserType
        /// <summary>
        /// Deletes an existing account type by its name.
        /// </summary>
        /// <remarks>
        /// This endpoint deletes an account type by its name. The name must be either "momo" or "card". 
        /// If the account type does not exist, a 404 response is returned. If an error occurs, a 500 response is returned.
        /// </remarks>
        /// <param name="Name">The name of the account type to delete.</param>
        /// <returns>An API response indicating the success or failure of the operation.</returns>
        /// <response code="200">User type successfully deleted.</response>
        /// <response code="400">Invalid user type name provided.</response>
        /// <response code="404">The specified account type was not found.</response>
        /// <response code="500">An error occurred while processing the request.</response>
        [HttpDelete("DeleteAccountType")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAccountType([FromHeader][Required] string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid user type data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "User type name is required" }
                });
            }
            Name = Name.ToLower();
            if (Name != "momo" && Name != "card")
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid user type",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "User type must be either 'momo' or 'card'" }
                });
            }

            try
            {
                var userType = await _context.TTypes!
                    .FirstOrDefaultAsync(e => e.Name!.ToLower() == Name);
                if (userType == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User type not found",
                        StatusCode = StatusCodes.Status404NotFound,
                        Errors = new[] { $"User type with name '{Name}' not found" }
                    });
                }

                _context.TTypes!.Remove(userType);


                await _context.SaveChangesAsync();
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "User Type Deleted Successfully",
                    StatusCode = StatusCodes.Status200OK
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
    #endregion

}
