using System.ComponentModel.DataAnnotations;
using dbContex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using ViewModel.Data;
using ViewModel.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hubtel.Api_Integration.Controllers
{
    /// <summary>
    /// Handles API requests related to card types such as adding, retrieving, updating, and deleting card types (Visa and MasterCard).
    /// </summary>
    /// <remarks>
    /// This controller supports the management of card types, with only "Visa" and "Master Card" as valid options. It allows administrators to add, retrieve, list, and delete card types in the system.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class CardTypeController : ControllerBase
    {
        private readonly HubtelWalletDbContext _context;

        /// <summary>
        ///  CardTypeController
        /// </summary>
        /// <param name="context"></param>
        public CardTypeController(HubtelWalletDbContext context)
        {
            _context = context;
        }


        #region AddCardType
        /// <summary>
        /// Adds a new card type (Visa or Master Card).
        /// </summary>
        /// <remarks>
        /// This endpoint adds a new card type to the system. Only "Visa" and "Master Card" are allowed as valid card types. 
        /// If a card type already exists, a conflict response is returned. If the data is invalid, a bad request response is returned.
        /// </remarks>
        /// <param name="cardType">The card type to add, including its name.</param>
        /// <returns>An API response indicating the success or failure of the operation.</returns>
        /// <response code="200">Card type successfully added.</response>
        /// <response code="400">Invalid card type data provided.</response>
        /// <response code="409">A card type with the same name already exists.</response>
        /// <response code="500">An error occurred while processing the request.</response>
        [HttpPost("AddCardType")]
        [ProducesResponseType(typeof(ApiResponse<ICardType>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCardType([Required][FromBody] ICardType cardType)
        {
            try
            {
                if (cardType == null || string.IsNullOrWhiteSpace(cardType.Name))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid Card type data",
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = new[] { "Card type name is required" }
                    });
                }

                var allowedCardTypes = new[] { "visa", "master card" };
                if (!allowedCardTypes.Contains(cardType.Name.ToLower()))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid card type",
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = new[] { "Card must be either 'visa' or 'master card'" }
                    });
                }

                var cardTypeExists = await _context.TCardTypes!
                    .AnyAsync(e => e.Name!.ToLower() == cardType.Name!.ToLower(), default);

                if (cardTypeExists)
                {
                    return Conflict(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Card type already exists",
                        StatusCode = StatusCodes.Status409Conflict,
                        Errors = new[] { $"A card type with name '{cardType.Name}' already exists" }
                    });
                }

                var cardTypeEntity = new TCardType
                {
                    Name = cardType.Name,
                    CreatedAt = DateTime.UtcNow,
                };

                await _context.TCardTypes!.AddAsync(cardTypeEntity);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<TCardType>
                {
                    Success = true,
                    Message = "Card Type Added Successfully",
                    StatusCode = StatusCodes.Status200OK,
                    Data = cardTypeEntity
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


        #region GetCardType
        /// <summary>
        /// Retrieves a specific card type by its name (Visa or Master Card).
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves a card type based on its name. Only "Visa" and "Master Card" are valid card types. 
        /// If the card type does not exist, a 404 response is returned.
        /// </remarks>
        /// <param name="Name">The name of the card type to retrieve (e.g., "Visa" or "Master Card").</param>
        /// <returns>The details of the requested card type.</returns>
        /// <response code="200">The requested card type was found.</response>
        /// <response code="400">Invalid card type name provided.</response>
        /// <response code="404">The requested card type was not found.</response>
        /// <response code="500">An error occurred while processing the request.</response>
        [HttpGet("GetCardTypeByName")]
        [ProducesResponseType(typeof(ApiResponse<CardType>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCardTypeByName([FromHeader][Required] string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid Card type data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "Card type name is required" }
                });
            }

            Name = Name.ToLower();
            var allowedCardTypes = new[] { "visa", "master card" };
            if (!allowedCardTypes.Contains(Name))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid Card type",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "Card type must be either 'visa' or 'master card'" }
                });
            }

            try
            {
                var cardType = await _context.TCardTypes!
                    .FirstOrDefaultAsync(e => e.Name!.ToLower() == Name);

                return cardType == null
                    ? NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Card type not found",
                        StatusCode = StatusCodes.Status404NotFound,
                        Errors = new[] { $"Card type with name '{Name}' not found" }
                    })
                    : Ok(new ApiResponse<TCardType>
                    {
                        Success = true,
                        Message = "Card Type Found",
                        StatusCode = StatusCodes.Status200OK,
                        Data = new TCardType() { Name = cardType.Name}

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


        #region GetAllCardTypes
        /// <summary>
        /// Retrieves all available card types (Visa and Master Card).
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of all available card types. If no card types are available, a 404 response is returned.
        /// </remarks>
        /// <returns>A list of all card types.</returns>
        /// <response code="200">All card types were found.</response>
        /// <response code="404">No card types found.</response>
        /// <response code="500">An error occurred while processing the request.</response>
        [HttpGet("GetAllCardTypes")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ICardType>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCardTypes()
        {
            try
            {
                var cardTypes = await _context.TCardTypes!.ToListAsync();

                return cardTypes.Count == 0
                    ? NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "No card types found",
                        StatusCode = StatusCodes.Status404NotFound,
                        Errors = new[] { "No card types found" }
                    })
                    : Ok(new ApiResponse<IEnumerable<ICardType>> 
                    {
                        Success = true,
                        Message = "Card Types Found",
                        StatusCode = StatusCodes.Status200OK,
                        Data = cardTypes.Select(c => new CardType { Name = c.Name }).ToList() 
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

        #region DeleteCardType
        /// <summary>
        /// Deletes an existing card type by its name (Visa or Master Card).
        /// </summary>
        /// <remarks>
        /// This endpoint deletes a card type from the system. The card type must be either "Visa" or "Master Card". 
        /// If the card type is not found, a 404 response is returned. If the card type has dependent data, a 400 response is returned.
        /// </remarks>
        /// <param name="Name">The name of the card type to delete (e.g., "Visa" or "MasterCard").</param>
        /// <returns>An API response indicating the success or failure of the operation.</returns>
        /// <response code="200">Card type successfully deleted.</response>
        /// <response code="400">Invalid card type name or card type has dependent data.</response>
        /// <response code="404">The specified card type was not found.</response>
        /// <response code="500">An error occurred while processing the request.</response>
        [HttpDelete("DeleteCardType")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCardType([FromHeader][Required] string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid card type data",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "Card type name is required" }
                });
            }

            Name = Name.ToLower();
            var allowedCardTypes = new[] { "visa", "master card" };
            if (!allowedCardTypes.Contains(Name))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid card type",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new[] { "Card type must be either 'visa' or 'master card'" }
                });
            }

            try
            {
                var cardType = await _context.TCardTypes!
                    .FirstOrDefaultAsync(e => e.Name!.ToLower() == Name);

                if (cardType == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Card type not found",
                        StatusCode = StatusCodes.Status404NotFound,
                        Errors = new[] { $"Card type with name '{Name}' not found" }
                    });
                }

                // Check for dependent data if needed
                if (await _context.TWalletAccountDetails!.AnyAsync(e => e.CardTypeId == cardType.Id))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Card type has dependent data",
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = new[] { $"Card type with name '{Name}' has dependent data" }
                    });
                }

                _context.TCardTypes!.Remove(cardType);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Card Type Deleted Successfully",
                    StatusCode = StatusCodes.Status200OK,
                    Data = $"Card Type {cardType.Name} Deleted Successfully!"
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