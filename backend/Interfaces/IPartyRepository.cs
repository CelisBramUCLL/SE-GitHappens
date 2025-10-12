using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Party;
using Dotnet_test.DTOs.Playlist;
using Dotnet_test.DTOs.Song;

namespace Dotnet_test.Interfaces
{
    public interface IPartyRepository
    {
        Task<List<PartyDTO>> GetAll();

        Task<PartyDTO?> GetById(int id);

        Task<PartyDTO> Create(Party party);

        Task<PartyDTO?> Update(Party party, UpdatePartyDTO request);

        Task<bool> Delete(int id);
        Task<ParticipantDTO> JoinParty(JoinPartyDTO dto, int loggedInUserId);
        Task<ParticipantInPartyDTO?> LeaveParty(int partyId, int loggedInUserId);
        Task<PlaylistSongDTO> AddSongToCurrentParty(int userId, AddSongDTO dto);
        Task<PlaylistSongDTO> RemoveSongFromCurrentParty(int userId, RemoveSongDTO dto);
        Task<PartyDTO?> GetUserActiveParty(int userId);
    }
}
