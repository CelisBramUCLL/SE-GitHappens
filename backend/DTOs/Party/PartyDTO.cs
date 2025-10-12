using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Playlist;
using Dotnet_test.DTOs.User;

namespace Dotnet_test.DTOs.Party
{
    public class PartyDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Status Status { get; set; } = Status.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public HostUserDTO HostUser { get; set; }

        public PlaylistDTO Playlist { get; set; }

        public List<ParticipantInPartyDTO> Participants { get; set; } =
            new List<ParticipantInPartyDTO>();
    }
}
