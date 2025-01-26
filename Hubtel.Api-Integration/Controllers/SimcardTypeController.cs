using System.ComponentModel.DataAnnotations;
using dbContex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModel.Data;
using ViewModel.Interfaces;

namespace Hubtel.Api_Integration.Controllers
{

    /// <summary>
    /// Handles API requests related to SIM card types such as adding, retrieving, updating, and deleting SIM card types (MTN, Vodafone, and Airteltigo).
    /// </summary>
    /// <remarks>
    /// This controller manages SIM card types in the system, restricting the types to "MTN", "Vodafone", and "Airteltigo". 
    /// It allows administrators to add, retrieve, list, and delete SIM card types.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class SimcardTypeController : Controller
    {
        private readonly HubtelWalletDbContext _context;

        /// <summary>
        ///  SimcardTypeController 
        /// </summary>
        /// <param name="context"></param>
        public SimcardTypeController(HubtelWalletDbContext context)
        {
            _context = context;
        }

        #region AddSimType
        /// <summary>
        /// Adds a new SIM card type (MTN, Vodafone, or Airteltigo).
        /// </summary>
        /// <remarks>
        /// This endpoint adds a new SIM card type to the system. Only "MTN", "Vodafone", and "Airteltigo" are allowed as valid SIM card types. 
        /// If a SIM card type already exists, a conflict response is returned. If the data is invalid, a bad request response is returned.
        /// </remarks>
        /// <param name="simType">The SIM card type to add, including its name.</param>
        /// <returns>An API response indicating the success or failure of the operation.</returns>
        /// <response code="200">SIM type successfully added.</response>
        /// <response code="400">Invalid SIM type data provided.</response>
        /// <response code="409">A SIM type with the same name already exists.</response>
        /// <response code="500">An error occurred while processing the request.</response>
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
        /// <summary>
        /// Retrieves a specific SIM card type by its name (MTN, Vodafone, or Airteltigo).
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves a SIM card type based on its name. Only "MTN", "Vodafone", and "Airteltigo" are valid SIM card types. 
        /// If the SIM card type does not exist, a 404 response is returned.
        /// </remarks>
        /// <param name="name">The name of the SIM card type to retrieve (e.g., "MTN", "Vodafone", or "Airteltigo").</param>
        /// <returns>The details of the requested SIM card type.</returns>
        /// <response code="200">The requested SIM card type was found.</response>
        /// <response code="400">Invalid SIM type name provided.</response>
        /// <response code="404">The requested SIM type was not found.</response>
        /// <response code="500">An error occurred while processing the request.</response>
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
        /// <summary>
        /// Retrieves all available SIM card types (MTN, Vodafone, Airteltigo).
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of all available SIM card types. If no SIM types are available, a 404 response is returned.
        /// </remarks>
        /// <returns>A list of all SIM card types.</returns>
        /// <response code="200">All SIM card types were found.</response>
        /// <response code="404">No SIM card types found.</response>
        /// <response code="500">An error occurred while processing the request.</response>
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
        /// <summary>
        /// Deletes an existing SIM card type by its name (MTN, Vodafone, or Airteltigo).
        /// </summary>
        /// <remarks>
        /// This endpoint deletes a SIM card type from the system. The SIM card type must be either "MTN", "Vodafone", or "Airteltigo". 
        /// If the SIM card type is not found, a 404 response is returned. If the SIM card type has dependent data, a 400 response is returned.
        /// </remarks>
        /// <param name="name">The name of the SIM card type to delete (e.g., "MTN", "Vodafone", or "Airteltigo").</param>
        /// <returns>An API response indicating the success or failure of the operation.</returns>
        /// <response code="200">SIM type successfully deleted.</response>
        /// <response code="400">Invalid SIM type name or SIM type has dependent data.</response>
        /// <response code="404">The specified SIM type was not found.</response>
        /// <response code="500">An error occurred while processing the request.</response>
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
