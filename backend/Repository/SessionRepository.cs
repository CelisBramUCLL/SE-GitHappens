using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Playlist;
using Dotnet_test.DTOs.Session;
using Dotnet_test.DTOs.Song;
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

            // Create playlist for the session
            var playlist = new Playlist
            {
                Name = $"{session.Name} Playlist",
                SessionId = session.Id,
            };
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            // Automatically add the host as a participant
            var hostParticipant = new Participant
            {
                SessionId = session.Id,
                UserId = session.HostUserId,
                JoinedAt = DateTime.UtcNow,
                Role = ParticipantRole.Host,
            };
            _context.Participants.Add(hostParticipant);
            await _context.SaveChangesAsync();

            // Load host user to populate DTO
            var hostUser = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == session.HostUserId
            );

            if (hostUser == null)
                throw new Exception("Host user not found");

            // Map to SessionDTO
            var sessionDto = new SessionDTO
            {
                Id = session.Id,
                Name = session.Name,
                Status = session.Status,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                HostUser = new HostUserDTO { Id = hostUser.Id, Username = hostUser.Username },
                Participants = new List<ParticipantInSessionDTO>
                {
                    new ParticipantInSessionDTO
                    {
                        Id = hostParticipant.Id,
                        UserId = session.HostUserId,
                        UserName = hostUser.Username,
                        JoinedAt = hostParticipant.JoinedAt,
                    },
                },
                Playlist = new PlaylistDTO
                {
                    Id = playlist.Id,
                    Name = playlist.Name,
                    Songs = new List<Dotnet_test.DTOs.Song.SongDTO>(),
                },
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
                .Include(s => s.Playlist)
                .ThenInclude(p => p.Songs)
                .ThenInclude(ps => ps.Song)
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
                    Playlist = new PlaylistDTO
                    {
                        Id = s.Playlist.Id,
                        Name = s.Playlist.Name,
                        Songs = s
                            .Playlist.Songs.Select(ps => new Dotnet_test.DTOs.Song.SongDTO
                            {
                                Id = ps.Song.Id,
                                Title = ps.Song.Title,
                                Artist = ps.Song.Artist,
                                Album = ps.Song.Album,
                                Duration = ps.Song.Duration,
                                FilePath = ps.Song.FilePath,
                                AddedBy = new HostUserDTO
                                {
                                    Id = ps.AddedByUser.Id,
                                    Username = ps.AddedByUser.Username,
                                },
                            })
                            .ToList(),
                    },
                })
                .ToListAsync();
        }

        public async Task<SessionDTO?> GetById(int id)
        {
            var session = await _context
                .Sessions.Include(s => s.HostUser)
                .Include(s => s.Participants)
                .ThenInclude(p => p.User)
                .Include(s => s.Playlist)
                .ThenInclude(p => p.Songs)
                .ThenInclude(ps => ps.Song)
                .Include(s => s.Playlist)
                .ThenInclude(p => p.Songs)
                .ThenInclude(ps => ps.AddedByUser)
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
                Playlist =
                    session.Playlist != null
                        ? new PlaylistDTO
                        {
                            Id = session.Playlist.Id,
                            Name = session.Playlist.Name,
                            Songs = session
                                .Playlist.Songs.Select(ps => new Dotnet_test.DTOs.Song.SongDTO
                                {
                                    Id = ps.Song.Id,
                                    Title = ps.Song.Title,
                                    Artist = ps.Song.Artist,
                                    Album = ps.Song.Album,
                                    Duration = ps.Song.Duration,
                                    FilePath = ps.Song.FilePath,
                                    AddedBy = new HostUserDTO
                                    {
                                        Id = ps.AddedByUser.Id,
                                        Username = ps.AddedByUser.Username,
                                    },
                                })
                                .ToList(),
                        }
                        : new PlaylistDTO
                        {
                            Id = 0,
                            Name = "No Playlist",
                            Songs = new List<Dotnet_test.DTOs.Song.SongDTO>(),
                        },
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

            // Update session properties
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

        public async Task<ParticipantDTO> JoinSession(JoinSessionDTO dto, int loggedInUserId)
        {
            // Check if session exists
            var session = await _context
                .Sessions.Include(s => s.Participants)
                .FirstOrDefaultAsync(s => s.Id == dto.SessionId);

            if (session == null)
                throw new Exception("Session not found");

            // Check if user is already a participant
            if (session.Participants.Any(p => p.UserId == loggedInUserId))
                throw new Exception("User already joined this session");

            // Load the user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == loggedInUserId);
            if (user == null)
                throw new Exception("User not found");

            // Create new participant
            var participant = new Participant
            {
                SessionId = dto.SessionId,
                UserId = loggedInUserId,
                JoinedAt = DateTime.UtcNow,
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            // Map to DTO
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

        public async Task<ParticipantInSessionDTO?> LeaveSession(int sessionId, int loggedInUserId)
        {
            // Find the participant entry for the given user and session
            var participant = await _context
                .Participants.Include(p => p.User) // include user to get Username
                .FirstOrDefaultAsync(p => p.SessionId == sessionId && p.UserId == loggedInUserId);

            if (participant == null)
                return null;

            // Remove the participant from session
            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            // Map to DTO
            var participantDto = new ParticipantInSessionDTO
            {
                Id = participant.Id,
                UserId = participant.UserId,
                UserName = participant.User.Username,
                JoinedAt = participant.JoinedAt,
            };

            return participantDto;
        }

        public async Task<PlaylistSongDTO> AddSongToCurrentSession(int userId, AddSongDTO dto)
        {
            // Find the active session the user is currently in (either as host or participant)
            var session = await _context
                .Sessions.Include(s => s.Playlist)
                .ThenInclude(p => p.Songs)
                .ThenInclude(ps => ps.Song)
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s =>
                    s.Status == Status.Active
                    && (s.HostUserId == userId || s.Participants.Any(p => p.UserId == userId))
                );

            if (session == null)
                throw new Exception("User is not in any active session");

            if (session.Playlist == null)
                throw new Exception("Session does not have a playlist");

            // Find the song to add
            var song = await _context.Songs.FirstOrDefaultAsync(s => s.Id == dto.SongId);
            if (song == null)
                throw new Exception("Song not found");

            // Determine the next position in the playlist
            int nextPosition = session.Playlist.Songs.Any()
                ? session.Playlist.Songs.Max(ps => ps.Position) + 1
                : 1;

            // Create the PlaylistSong entry
            var playlistSong = new PlaylistSong
            {
                PlaylistId = session.Playlist.Id,
                SongId = song.Id,
                AddedByUserId = userId,
                Position = nextPosition,
                AddedAt = DateTime.UtcNow,
            };

            _context.PlaylistSongs.Add(playlistSong);
            await _context.SaveChangesAsync();

            // Map to DTO
            return new PlaylistSongDTO
            {
                Id = playlistSong.Id,
                SongId = song.Id,
                Title = song.Title,
                Artist = song.Artist,
                Album = song.Album,
                Duration = song.Duration,
                FilePath = song.FilePath,
                AddedByUserId = userId,
                Position = nextPosition,
                AddedAt = playlistSong.AddedAt,
            };
        }

        public async Task<PlaylistSongDTO> RemoveSongFromCurrentSession(
            int userId,
            RemoveSongDTO dto
        )
        {
            // Find the active session the user is currently in (either as host or participant)
            var session = await _context
                .Sessions.Include(s => s.Playlist)
                .ThenInclude(p => p.Songs)
                .ThenInclude(ps => ps.Song)
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s =>
                    s.Status == Status.Active
                    && (s.HostUserId == userId || s.Participants.Any(p => p.UserId == userId))
                );

            if (session == null)
                throw new Exception("User is not in any active session");

            if (session.Playlist == null)
                throw new Exception("Session does not have a playlist");

            // Find the playlist entry
            var playlistSong = session.Playlist.Songs.FirstOrDefault(ps => ps.SongId == dto.SongId);

            if (playlistSong == null)
                throw new Exception("Song not found in the playlist");

            // Remove the song
            _context.PlaylistSongs.Remove(playlistSong);
            await _context.SaveChangesAsync();

            // Map to DTO
            var playlistSongDto = new PlaylistSongDTO
            {
                Id = playlistSong.Id,
                SongId = playlistSong.SongId,
                Title = playlistSong.Song.Title,
                Artist = playlistSong.Song.Artist,
                Album = playlistSong.Song.Album,
                Duration = playlistSong.Song.Duration,
                FilePath = playlistSong.Song.FilePath,
                AddedByUserId = playlistSong.AddedByUserId,
                Position = playlistSong.Position,
                AddedAt = playlistSong.AddedAt,
            };

            return playlistSongDto;
        }
    }
}
