using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.User;

namespace Dotnet_test.DTOs.Session
{
    public class SessionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Status Status { get; set; } = Status.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Host user (FK)
        public HostUserDTO HostUser { get; set; }

        public List<ParticipantInSessionDTO> Participants { get; set; } =
            new List<ParticipantInSessionDTO>();
    }
}
