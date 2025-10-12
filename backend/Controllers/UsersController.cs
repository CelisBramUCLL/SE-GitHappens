using Dotnet_test.Domain;
using Dotnet_test.DTOs.Product;
using Dotnet_test.Interfaces;
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

        // full list
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _userRepository.GetAll();

            return Ok(response);
        }

        // single by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var response = await _userRepository.GetById(id);

            return Ok(response);
        }

        // create
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDTO request)
        {
            var newUser = new User()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = request.Password,
                Username = request.Username,
                Role = Enum.TryParse<Role>(request.Role, out var parsedRole)
                    ? parsedRole
                    : Role.User, // fallback to Role.User if parsing fails
            };

            var created = await _userRepository.Create(newUser);

            return Ok(created);
        }

        // update
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
                    : Role.User, // fallback to Role.User if parsing fails
            };

            var updated = await _userRepository.Update(userToUpdate, request);

            if (updated == null)
                return NotFound("User not found");

            return Ok(updated);
        }

        // delete by Id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userRepository.Delete(id);

            if (!deleted)
                return NotFound($"User with id {id} not found");

            return Ok($"User with id {id} deleted");
        }
    }
}
