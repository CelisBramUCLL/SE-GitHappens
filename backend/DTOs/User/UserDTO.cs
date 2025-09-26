using Dotnet_test.Domain;

namespace Dotnet_test.DTOs.Product
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public Role Role { get; set; }
    }
}
