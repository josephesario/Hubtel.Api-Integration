using System.ComponentModel.DataAnnotations;
using dbContex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModel.Data;
using ViewModel.Interfaces;

namespace Hubtel.Api_Integration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardTypeController : ControllerBase
    {
        private readonly HubtelWalletDbContextExtended _context;

        public CardTypeController(HubtelWalletDbContextExtended context)
        {
            _context = context;
        }


        #region AddCardType
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
        [HttpGet("GetCardTypeByName")]
        [ProducesResponseType(typeof(ApiResponse<TCardType>), StatusCodes.Status200OK)]
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
                        Data = cardType
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
        [HttpGet("GetAllCardTypes")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TCardType>>), StatusCodes.Status200OK)]
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
                    : Ok(new ApiResponse<IEnumerable<TCardType>>
                    {
                        Success = true,
                        Message = "Card Types Found",
                        StatusCode = StatusCodes.Status200OK,
                        Data = cardTypes
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
                if (await _context.TCardAccountDetails!.AnyAsync(e => e.CardTypeId == cardType.Id))
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
}