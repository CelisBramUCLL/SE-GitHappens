using Dotnet_test.Domain;
using Dotnet_test.DTOs.User;
using Dotnet_test.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Get all users
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _userRepository.GetAll();

            return Ok(response);
        }

        // Get user by ID
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var response = await _userRepository.GetById(id);

            return Ok(response);
        }

        // Create new user
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDTO request)
        {
            var newUser = new User()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Username = request.Username,
                Role = Enum.TryParse<Role>(request.Role, out var parsedRole)
                    ? parsedRole
                    : Role.User,
            };

            var created = await _userRepository.Create(newUser);

            return Ok(created);
        }

        // Update user
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDTO request)
        {
            var userToUpdate = new User
            {
                Id = id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = request.Password,
                Username = request.Username,
                Role = Enum.TryParse<Role>(request.Role, out var parsedRole)
                    ? parsedRole
                    : Role.User,
            };

            var updated = await _userRepository.Update(userToUpdate, request);

            if (updated == null)
                return NotFound("User not found");

            return Ok(updated);
        }

        // Delete user by ID
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userRepository.Delete(id);

            if (!deleted)
                return NotFound($"User with id {id} not found");

            return Ok($"User with id {id} deleted");
        }

        // User login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginDTO dto)
        {
            var result = await _userRepository.Login(dto);
            if (result == null)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(result);
        }
    }
}
