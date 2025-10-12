using Dotnet_test.DTOs.User;

namespace Dotnet_test.DTOs.Participant
{
    public class ParticipantDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SessionId { get; set; }
        public DateTime JoinedAt { get; set; }

        public HostUserDTO? User { get; set; }
    }
}
