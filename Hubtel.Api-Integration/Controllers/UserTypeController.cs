using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hubtel.Api_Integration.Controllers
{
    #region AccessController
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class UserTypeController : ControllerBase
    {

        #region AddUserType
        [HttpPost("add")]
        public async Task<IActionResult> AddUserType([FromBody] UserType userType)
        {
            try
            {
                if (userType == null)
                {
                    return BadRequest("UserType is null");
                }
                var userTypeExists = await _context.TUserTypes!.AnyAsync(e => e.Name!.ToLower() == userType.Name!.ToLower());
                if (userTypeExists)
                {
                    return BadRequest("UserType already exists");
                }
                var userTypeEntity = new TUserType
                {
                    Name = userType.Name,
                    Description = userType.Description,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _context.TUserTypes!.AddAsync(userTypeEntity);
                await _context.SaveChangesAsync();
                return Ok("UserType added successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        #endregion


    }
    #endregion
}
