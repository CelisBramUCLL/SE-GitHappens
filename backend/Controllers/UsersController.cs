using Dotnet_test.DTOs.User;
using Dotnet_test.Interfaces;
using Dotnet_test.Services;      
using Dotnet_test.Exceptions; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _userService.GetAllAsync(); 
            return Ok(response);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var response = await _userService.GetByIdAsync(id); 

            if (response == null) return NotFound(); 
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDTO request)
        {
            try
            {
                var created = await _userService.CreateUserAsync(request);

                return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
            }
            catch (EmailAlreadyExistsException ex) 
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDTO request)
        {
            var updated = await _userService.UpdateUserAsync(id, request);

            if (updated == null)
                return NotFound("User not found");

            return Ok(updated);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userService.DeleteUserAsync(id); 

            if (!deleted)
                return NotFound($"User with id {id} not found");

            return Ok($"User with id {id} deleted");
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO dto)
        {
            var result = await _userService.LoginAsync(dto);
            
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(result);
        }
    }
}