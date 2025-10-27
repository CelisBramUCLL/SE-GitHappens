using Dotnet_test.Domain;
using Dotnet_test.DTOs.User;

namespace Dotnet_test.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetByIdAsync(int id);
        Task<User> CreateUserAsync(CreateUserDTO request);
        Task<User> UpdateUserAsync(int id, UpdateUserDTO request);
        Task<bool> DeleteUserAsync(int id);
        Task<LoginResponseDTO> LoginAsync(LoginDTO dto);
    }
}