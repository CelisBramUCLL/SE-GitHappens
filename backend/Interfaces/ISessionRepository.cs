using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Playlist;
using Dotnet_test.DTOs.Session;
using Dotnet_test.DTOs.Song;

namespace Dotnet_test.Interfaces
{
    public interface ISessionRepository
    {
        Task<List<SessionDTO>> GetAll();

        Task<SessionDTO?> GetById(int id);

        Task<SessionDTO> Create(Session session);

        Task<SessionDTO?> Update(Session session, UpdateSessionDTO request);

        Task<bool> Delete(int id);
        Task<ParticipantDTO> JoinSession(JoinSessionDTO dto, int loggedInUserId);
        Task<ParticipantInSessionDTO?> LeaveSession(int sessionId, int loggedInUserId);
        Task<PlaylistSongDTO> AddSongToCurrentSession(int userId, AddSongDTO dto);
        Task<PlaylistSongDTO> RemoveSongFromCurrentSession(int userId, RemoveSongDTO dto);
    }
}
