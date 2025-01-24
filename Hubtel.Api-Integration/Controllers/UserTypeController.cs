using dbContex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModel.Data;
using ViewModel.Interfaces;

namespace Hubtel.Api_Integration.Controllers
{
    #region AccessController
    [ApiController]
    [Route("api/[controller]")]
    public class UserTypeController : ControllerBase
    {

        private readonly HubtelWalletDbContextExtended _context;

        public UserTypeController(HubtelWalletDbContextExtended context)
        {
            _context = context;
        }

        #region AddUserType
        [HttpPost("add")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddUserType([FromBody] IUserType userType)
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

                var userTypeExists = await _context.TUserTypes!
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

                var userTypeEntity = new TUserType
                {
                    Name = userType.Name,
                    CreatedAt = DateTime.UtcNow,
                };

                await _context.TUserTypes!.AddAsync(userTypeEntity);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<TUserType>
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

    }
    #endregion
}
