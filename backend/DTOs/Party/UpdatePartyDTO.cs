using Dotnet_test.Domain;

namespace Dotnet_test.DTOs.Party
{
    public class UpdatePartyDTO
    {
        public string? Name { get; set; }
        public Status? Status { get; set; }
    }
}
