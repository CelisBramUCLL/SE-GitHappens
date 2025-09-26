using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Session;
using Dotnet_test.DTOs.User;
using Dotnet_test.Infrastructure;
using Dotnet_test.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dotnet_test.Repository
{
    public class SessionRepository : ISessionRepository
    {
        private readonly ApplicationDbContext _context;

        public SessionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SessionDTO> Create(Session session)
        {
            // Add session to the database
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            // Load host user to populate DTO
            var hostUser = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == session.HostUserId
            );

            // Map to DTO
            var sessionDto = new SessionDTO
            {
                Id = session.Id,
                Name = session.Name,
                Status = session.Status,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                HostUser = new HostUserDTO { Id = hostUser.Id, Username = hostUser.Username },
                Participants = new List<ParticipantInSessionDTO>(), // empty at creation
            };

            return sessionDto;
        }

        public async Task<bool> Delete(int id)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(x => x.Id == id);
            if (session == null)
                return false;

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SessionDTO>> GetAll()
        {
            return await _context
                .Sessions.Include(s => s.HostUser)
                .Include(s => s.Participants)
                .ThenInclude(p => p.User)
                .Select(s => new SessionDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    HostUser = new HostUserDTO
                    {
                        Id = s.HostUser.Id,
                        Username = s.HostUser.Username,
                    },
                    Participants = s
                        .Participants.Select(p => new ParticipantInSessionDTO
                        {
                            Id = p.Id,
                            UserId = p.UserId,
                            JoinedAt = p.JoinedAt,
                            UserName = p.User.Username,
                        })
                        .ToList(),
                })
                .ToListAsync();
        }

        public async Task<SessionDTO?> GetById(int id)
        {
            var session = await _context
                .Sessions.Include(s => s.HostUser)
                .Include(s => s.Participants)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
                return null;

            var sessionDto = new SessionDTO
            {
                Id = session.Id,
                Name = session.Name,
                Status = session.Status,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                HostUser = new HostUserDTO
                {
                    Id = session.HostUser.Id,
                    Username = session.HostUser.Username,
                },
                Participants = session
                    .Participants.Select(p => new ParticipantInSessionDTO
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        JoinedAt = p.JoinedAt,
                        UserName = p.User.Username,
                    })
                    .ToList(),
            };

            return sessionDto;
        }

        public async Task<SessionDTO?> Update(Session session, UpdateSessionDTO request)
        {
            var sessionInDb = await _context
                .Sessions.Include(s => s.HostUser)
                .Include(s => s.Participants)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(x => x.Id == session.Id);

            if (sessionInDb == null)
                return null;

            // Update properties
            if (request.Name != null)
                sessionInDb.Name = request.Name;

            if (request.Status != null)
                sessionInDb.Status = request.Status.Value;

            sessionInDb.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Map to DTO
            var sessionDto = new SessionDTO
            {
                Id = sessionInDb.Id,
                Name = sessionInDb.Name,
                Status = sessionInDb.Status,
                CreatedAt = sessionInDb.CreatedAt,
                UpdatedAt = sessionInDb.UpdatedAt,
                HostUser = new HostUserDTO
                {
                    Id = sessionInDb.HostUser.Id,
                    Username = sessionInDb.HostUser.Username,
                },
                Participants = sessionInDb
                    .Participants.Select(p => new ParticipantInSessionDTO
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        JoinedAt = p.JoinedAt,
                        UserName = p.User.Username,
                    })
                    .ToList(),
            };

            return sessionDto;
        }

        public async Task<ParticipantDTO> JoinSession(JoinSessionDTO dto)
        {
            // 1️⃣ Check if session exists
            var session = await _context
                .Sessions.Include(s => s.Participants) // include existing participants
                .FirstOrDefaultAsync(s => s.Id == dto.SessionId);

            if (session == null)
                throw new Exception("Session not found");

            // 2️⃣ Check if user is already a participant
            if (session.Participants.Any(p => p.UserId == dto.UserId))
                throw new Exception("User already joined this session");

            // 3️⃣ Load the user separately
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);

            if (user == null)
                throw new Exception("User not found");

            // 4️⃣ Create new participant
            var participant = new Participant
            {
                SessionId = dto.SessionId,
                UserId = dto.UserId,
                JoinedAt = DateTime.UtcNow,
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            // 5️⃣ Map to DTO
            var participantDto = new ParticipantDTO
            {
                Id = participant.Id,
                SessionId = participant.SessionId,
                UserId = participant.UserId,
                JoinedAt = participant.JoinedAt,
                User = new HostUserDTO { Id = user.Id, Username = user.Username },
            };

            return participantDto;
        }
    }
}
