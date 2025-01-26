using System.ComponentModel.DataAnnotations;
using dbContex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModel.Data;
using ViewModel.Interfaces;

namespace Hubtel.Api_Integration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimcardTypeController : Controller
    {
        private readonly HubtelWalletDbContext _context;

        public SimcardTypeController(HubtelWalletDbContext context)
        {
            _context = context;
        }

        #region AddSimType
        [HttpPost("AddSimType")]
        [ProducesResponseType(typeof(ApiResponse<ISimcardType>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddSimType([Required][FromBody] ISimcardType simType)
        {
            try
            {
                if (simType == null || string.IsNullOrWhiteSpace(simType.Name))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid SIM type data",
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = new[] { "SIM type name is required" }
                    });
                }

                var allowedSimTypes = new[] { "vodafone", "mtn", "airteltigo" };
                if (!allowedSimTypes.Contains(simType.Name.ToLower()))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid SIM type",
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = new[] { "SIM type must be 'vodafone', 'mtn', or 'airteltigo'" }
                    });
                }

                var simTypeExists = await _context.TSimcardTypes!
                    .AnyAsync(e => e.Name!.ToLower() == simType.Name!.ToLower());

                if (simTypeExists)
                {
                    return Conflict(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "SIM type already exists",
                        StatusCode = StatusCodes.Status409Conflict,
                        Errors = new[] { $"A SIM type with name '{simType.Name}' already exists" }
                    });
                }

                var simTypeEntity = new TSimcardType
                {
                    Name = simType.Name,
                    CreatedAt = DateTime.UtcNow,
                };

                await _context.TSimcardTypes!.AddAsync(simTypeEntity);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<TSimcardType>
                {
                    Success = true,
                    Message = "SIM Type Added Successfully",
                    StatusCode = StatusCodes.Status200OK,
                    Data = simTypeEntity
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

        #region GetSimTypeByName
        [HttpGet("GetSimTypeByName")]
        [ProducesResponseType(typeof(ApiResponse<SimcardType>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSimTypeByName([FromHeader][Required] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid SIM type data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "SIM type name is required" }
                });
            }

            name = name.ToLower();
            var allowedSimTypes = new[] { "vodafone", "mtn", "airteltigo" };
            if (!allowedSimTypes.Contains(name))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid SIM type",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "SIM type must be 'vodafone', 'mtn', or 'airteltigo'" }
                });
            }

            try
            {
                var simType = await _context.TSimcardTypes!
                    .FirstOrDefaultAsync(e => e.Name!.ToLower() == name);

                return simType == null
                    ? NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "SIM type not found",
                        StatusCode = StatusCodes.Status404NotFound,
                        Errors = new[] { $"SIM type with name '{name}' not found" }
                    })
                    : Ok(new ApiResponse<SimcardType>
                    {
                        Success = true,
                        Message = "SIM Type Found",
                        StatusCode = StatusCodes.Status200OK,
                        Data = new SimcardType() { Name = simType.Name }
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

        #region GetAllSimTypes
        [HttpGet("GetAllSimTypes")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ISimcardType>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSimTypes()
        {
            try
            {
                var simTypes = await _context.TSimcardTypes!.ToListAsync();

                return simTypes.Count == 0
                    ? NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "No SIM types found",
                        StatusCode = StatusCodes.Status404NotFound,
                        Errors = new[] { "No data available" }
                    })
                    : Ok(new ApiResponse<IEnumerable<ISimcardType>>
                    {
                        Success = true,
                        Message = "SIM Types Found",
                        StatusCode = StatusCodes.Status200OK,
                        Data = simTypes.Select(sim => new SimcardType { Name = sim.Name })
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

        #region DeleteSimType
        [HttpDelete("DeleteSimType")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSimType([FromHeader][Required] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid SIM type data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "SIM type name is required" }
                });
            }

            name = name.ToLower();

            try
            {

                var simType = await _context.TSimcardTypes!
                    .FirstOrDefaultAsync(e => e.Name!.ToLower() == name);

                if (simType == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "SIM type not found",
                        StatusCode = StatusCodes.Status404NotFound,
                        Errors = new[] { $"SIM type with name '{name}' not found" }
                    });
                }

                var hasDependents = await _context.TWalletAccountDetails!
                    .AnyAsync(e => e.SimCardTypeId == simType.Id); 

                if (hasDependents)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Cannot delete SIM type because it has dependent records",
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = new[] { $"SIM type '{name}' is referenced in other records and cannot be deleted" }
                    });
                }

                _context.TSimcardTypes!.Remove(simType);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "SIM Type Deleted Successfully",
                    StatusCode = StatusCodes.Status200OK,
                    Data = $"SIM type '{name}' has been deleted successfully"
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
