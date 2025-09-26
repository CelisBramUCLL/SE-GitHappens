using Dotnet_test.Domain;

namespace Dotnet_test.DTOs.Session
{
    public class UpdateSessionDTO
    {
        public string? Name { get; set; }
        public Status? Status { get; set; }
    }
}
