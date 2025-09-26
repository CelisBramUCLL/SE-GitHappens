using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Session;

namespace Dotnet_test.Interfaces
{
    public interface ISessionRepository
    {
        Task<List<SessionDTO>> GetAll();

        Task<Session?> GetById(int id);

        Task<SessionDTO> Create(Session session);

        Task<Session?> Update(Session session, UpdateSessionDTO request);

        Task<bool> Delete(int id);
        Task<ParticipantDTO> JoinSession(JoinSessionDTO dto);
    }
}
