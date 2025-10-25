using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Party;
using Dotnet_test.DTOs.Song;

namespace Dotnet_test.Interfaces
{
    public interface IPartyService
    {
        Task<IEnumerable<Party>> GetAllAsync();
        Task<Party> GetByIdAsync(int id);
        
        // El servicio necesita el DTO y el ID del usuario que crea la fiesta
        Task<PartyDTO> CreatePartyAsync(CreatePartyDTO dto, int userId);
        
        Task<Party> UpdatePartyAsync(int id, UpdatePartyDTO dto);
        
        // El servicio necesita el ID de la fiesta y el ID del usuario que la borra
        Task<bool> DeletePartyAsync(int id, int userId);
        
        Task<Participant> JoinPartyAsync(JoinPartyDTO dto, int userId);
        Task<Participant> LeavePartyAsync(int partyId, int userId);
        Task<PlaylistSong> AddSongAsync(AddSongDTO dto, int userId);
        Task<PlaylistSong> RemoveSongAsync(RemoveSongDTO dto, int userId);
        Task<Party> GetMyActivePartyAsync(int userId);
    }
}