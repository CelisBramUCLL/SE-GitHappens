using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Party;
using Dotnet_test.DTOs.Playlist;

using Dotnet_test.DTOs.Song;

namespace Dotnet_test.Interfaces
{
    public interface IPartyService
    {
        Task<IEnumerable<PartyDTO>> GetAllAsync();
        Task<PartyDTO> GetByIdAsync(int id);
        
        // El servicio necesita el DTO y el ID del usuario que crea la fiesta
        Task<PartyDTO> CreatePartyAsync(CreatePartyDTO dto, int userId);
        
        Task<PartyDTO> UpdatePartyAsync(int id, UpdatePartyDTO dto);
        
        // El servicio necesita el ID de la fiesta y el ID del usuario que la borra
        Task<bool> DeletePartyAsync(int id, int userId);
        
        Task<ParticipantDTO> JoinPartyAsync(JoinPartyDTO dto, int userId);
        Task<ParticipantInPartyDTO> LeavePartyAsync(int partyId, int userId);
        Task<PlaylistSongDTO> AddSongAsync(AddSongDTO dto, int userId);
        Task<PlaylistSongDTO> RemoveSongAsync(RemoveSongDTO dto, int userId);
        Task<PartyDTO> GetMyActivePartyAsync(int userId);
    }
}