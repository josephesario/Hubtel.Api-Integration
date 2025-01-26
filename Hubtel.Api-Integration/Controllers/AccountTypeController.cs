using System;
using System.ComponentModel.DataAnnotations;
using dbContex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModel.Data;
using ViewModel.Interfaces;

namespace Hubtel.Api_Integration.Controllers
{
    #region AccountTypeController
    [ApiController]
    [Route("api/[controller]")]
    public class AccountTypeController : ControllerBase
    {

        private readonly HubtelWalletDbContext _context;

        public AccountTypeController(HubtelWalletDbContext context)
        {
            _context = context;
        }

        #region AddUserType
        [HttpPost("AddAccountType")]
        [ProducesResponseType(typeof(ApiResponse<IAccountType>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AccountType([Required][FromBody] IAccountType userType)
        {
            try
            {
                if (userType == null || string.IsNullOrWhiteSpace(userType.Name))
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
                if (!allowedUserTypes.Contains(userType.Name.ToLower()))
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
                    .AnyAsync(e => e.Name!.ToLower() == userType.Name!.ToLower(), default);

                if (userTypeExists)
                {
                    return Conflict(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User type already exists",
                        StatusCode = StatusCodes.Status409Conflict,
                        Errors = new[] { $"A user type with name '{userType.Name}' already exists" }
                    });
                }

                var userTypeEntity = new TType
                {
                    Name = userType.Name,
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
