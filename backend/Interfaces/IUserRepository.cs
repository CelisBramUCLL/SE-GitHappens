using Dotnet_test.Domain;
using Dotnet_test.DTOs.User;

namespace Dotnet_test.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAll();
        Task<User?> GetById(int id);
        Task<User> Create(User user);
        Task<User?> Update(User user, UpdateUserDTO request);
        Task<bool> Delete(int id);
        Task<LoginResponseDTO?> Login(LoginDTO dto);
    }
}
