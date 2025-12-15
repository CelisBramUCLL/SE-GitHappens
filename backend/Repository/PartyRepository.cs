using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Party;
using Dotnet_test.DTOs.Playlist;
using Dotnet_test.DTOs.Song;
using Dotnet_test.DTOs.User;
using Dotnet_test.Infrastructure;
using Dotnet_test.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dotnet_test.Repository
{
    public class PartyRepository : GenericRepository<Party, int>, IPartyRepository
    {
        public PartyRepository(ApplicationDbContext context)
            : base(context) { }

        public async Task<PartyDTO> Create(Party party)
        {
            // Check if user already hosts an active party
            var existingHostedParty = await _context.Parties.FirstOrDefaultAsync(p =>
                p.HostUserId == party.HostUserId && p.Status == Status.Active
            );

            if (existingHostedParty != null)
                throw new Exception("User is already hosting an active party");

            // Check if user is already participating in any active party
            var existingParticipation = await _context
                .Participants.Include(p => p.Party)
                .FirstOrDefaultAsync(p =>
                    p.UserId == party.HostUserId && p.Party.Status == Status.Active
                );

            if (existingParticipation != null)
                throw new Exception("User is already participating in an active party");

            // Add party to the database using inherited generic method
            await AddAsync(party);

            // Create playlist for the party
            var playlist = new Playlist { Name = $"{party.Name} Playlist", PartyId = party.Id };
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            // Automatically add the host as a participant
            var hostParticipant = new Participant
            {
                PartyId = party.Id,
                UserId = party.HostUserId,
                JoinedAt = DateTime.UtcNow,
                Role = ParticipantRole.Host,
            };
            _context.Participants.Add(hostParticipant);
            await _context.SaveChangesAsync();

            // Load host user to populate DTO
            var hostUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == party.HostUserId);

            if (hostUser == null)
                throw new Exception("Host user not found");

            // Map to PartyDTO
            var partyDto = new PartyDTO
            {
                Id = party.Id,
                Name = party.Name,
                Status = party.Status,
                CreatedAt = party.CreatedAt,
                UpdatedAt = party.UpdatedAt,
                CurrentlyPlayingSongId = party.CurrentlyPlayingSongId,
                IsPlaying = party.IsPlaying,
                CurrentPosition = party.CurrentPosition,
                HostUser = new HostUserDTO(hostUser.Id, hostUser.Username),
                Participants = new List<ParticipantInPartyDTO>
                {
                    new ParticipantInPartyDTO
                    {
                        Id = hostParticipant.Id,
                        UserId = party.HostUserId,
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

            return partyDto;
        }

        public async Task<bool> Delete(int id)
        {
            // Use inherited generic method
            return await DeleteAsync(id);
        }

        public async Task<List<PartyDTO>> GetAll()
        {
            return await _context
                .Parties.Include(s => s.HostUser)
                .Include(s => s.Participants)
                .ThenInclude(p => p.User)
                .Include(s => s.Playlist)
                .ThenInclude(p => p.Songs)
                .ThenInclude(ps => ps.Song)
                .Select(s => new PartyDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    CurrentlyPlayingSongId = s.CurrentlyPlayingSongId,
                    IsPlaying = s.IsPlaying,
                    CurrentPosition = s.CurrentPosition,
                    HostUser = new HostUserDTO(s.HostUser.Id, s.HostUser.Username),
                    Participants = s
                        .Participants.Select(p => new ParticipantInPartyDTO
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
                                AddedBy = new HostUserDTO(
                                    ps.AddedByUser.Id,
                                    ps.AddedByUser.Username
                                ),
                            })
                            .ToList(),
                    },
                })
                .ToListAsync();
        }

        public async Task<PartyDTO?> GetById(int id)
        {
            var party = await _context
                .Parties.Include(s => s.HostUser)
                .Include(s => s.Participants)
                .ThenInclude(p => p.User)
                .Include(s => s.Playlist)
                .ThenInclude(p => p.Songs)
                .ThenInclude(ps => ps.Song)
                .Include(s => s.Playlist)
                .ThenInclude(p => p.Songs)
                .ThenInclude(ps => ps.AddedByUser)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (party == null)
                return null;

            var partyDto = new PartyDTO
            {
                Id = party.Id,
                Name = party.Name,
                Status = party.Status,
                CreatedAt = party.CreatedAt,
                UpdatedAt = party.UpdatedAt,
                CurrentlyPlayingSongId = party.CurrentlyPlayingSongId,
                IsPlaying = party.IsPlaying,
                CurrentPosition = party.CurrentPosition,
                HostUser = new HostUserDTO(party.HostUser.Id, party.HostUser.Username),
                Participants = party
                    .Participants.Select(p => new ParticipantInPartyDTO
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        JoinedAt = p.JoinedAt,
                        UserName = p.User.Username,
                    })
                    .ToList(),
                Playlist =
                    party.Playlist != null
                        ? new PlaylistDTO
                        {
                            Id = party.Playlist.Id,
                            Name = party.Playlist.Name,
                            Songs = party
                                .Playlist.Songs.Select(ps => new Dotnet_test.DTOs.Song.SongDTO
                                {
                                    Id = ps.Song.Id,
                                    Title = ps.Song.Title,
                                    Artist = ps.Song.Artist,
                                    Album = ps.Song.Album,
                                    Duration = ps.Song.Duration,
                                    FilePath = ps.Song.FilePath,
                                    AddedBy = new HostUserDTO(
                                        ps.AddedByUser.Id,
                                        ps.AddedByUser.Username
                                    ),
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

            return partyDto;
        }

        // TODO: Add remaining methods (Update, JoinParty, LeaveParty, AddSongToCurrentParty, RemoveSongFromCurrentParty)
        // These will be implemented as we continue the conversion

        public async Task<PartyDTO?> Update(Party party, UpdatePartyDTO request)
        {
            var partyInDb = await _context
                .Parties.Include(s => s.HostUser)
                .Include(s => s.Participants)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(x => x.Id == party.Id);

            if (partyInDb == null)
                return null;

            // Update party properties
            if (request.Name != null)
                partyInDb.Name = request.Name;

            if (request.Status != null)
                partyInDb.Status = request.Status.Value;

            partyInDb.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Map to DTO
            var partyDto = new PartyDTO
            {
                Id = partyInDb.Id,
                Name = partyInDb.Name,
                Status = partyInDb.Status,
                CreatedAt = partyInDb.CreatedAt,
                UpdatedAt = partyInDb.UpdatedAt,
                CurrentlyPlayingSongId = partyInDb.CurrentlyPlayingSongId,
                IsPlaying = partyInDb.IsPlaying,
                CurrentPosition = partyInDb.CurrentPosition,
                HostUser = new HostUserDTO(partyInDb.HostUser.Id, partyInDb.HostUser.Username),
                Participants = partyInDb
                    .Participants.Select(p => new ParticipantInPartyDTO
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        JoinedAt = p.JoinedAt,
                        UserName = p.User.Username,
                    })
                    .ToList(),
            };

            return partyDto;
        }

        public async Task<ParticipantDTO> JoinParty(JoinPartyDTO dto, int loggedInUserId)
        {
            // Check if party exists
            var party = await _context
                .Parties.Include(s => s.Participants)
                .FirstOrDefaultAsync(s => s.Id == dto.PartyId);

            if (party == null)
                throw new Exception("Party not found");

            // Check if party is active
            if (party.Status != Status.Active)
                throw new Exception("Cannot join an inactive party");

            // Check if user is already a participant in this party
            if (party.Participants.Any(p => p.UserId == loggedInUserId))
                throw new Exception("User already joined this party");

            // Check if user is already hosting any active party
            var existingHostedParty = await _context.Parties.FirstOrDefaultAsync(p =>
                p.HostUserId == loggedInUserId && p.Status == Status.Active
            );

            if (existingHostedParty != null)
                throw new Exception(
                    "User is already hosting an active party and cannot join another"
                );

            // Check if user is already participating in any other active party
            var existingParticipation = await _context
                .Participants.Include(p => p.Party)
                .FirstOrDefaultAsync(p =>
                    p.UserId == loggedInUserId && p.Party.Status == Status.Active
                );

            if (existingParticipation != null)
                throw new Exception("User is already participating in an active party");

            // Load the user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == loggedInUserId);
            if (user == null)
                throw new Exception("User not found");

            // Create new participant
            var participant = new Participant
            {
                PartyId = dto.PartyId,
                UserId = loggedInUserId,
                JoinedAt = DateTime.UtcNow,
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            // Map to DTO
            var participantDto = new ParticipantDTO
            {
                Id = participant.Id,
                PartyId = participant.PartyId,
                UserId = participant.UserId,
                JoinedAt = participant.JoinedAt,
                User = new HostUserDTO(user.Id, user.Username),
            };

            return participantDto;
        }

        public async Task<ParticipantInPartyDTO?> LeaveParty(int partyId, int loggedInUserId)
        {
            // Find the participant entry for the given user and party
            var participant = await _context
                .Participants.Include(p => p.User) // include user to get Username
                .FirstOrDefaultAsync(p => p.PartyId == partyId && p.UserId == loggedInUserId);

            if (participant == null)
                return null;

            // Remove the participant from party
            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            // Map to DTO
            var participantDto = new ParticipantInPartyDTO
            {
                Id = participant.Id,
                UserId = participant.UserId,
                UserName = participant.User.Username,
                JoinedAt = participant.JoinedAt,
            };

            return participantDto;
        }

        public async Task<PlaylistSongDTO> AddSongToCurrentParty(int userId, AddSongDTO dto)
        {
            // Find the active party the user is currently in (either as host or participant)
            var party = await _context
                .Parties.Include(s => s.Playlist)
                .ThenInclude(p => p.Songs)
                .ThenInclude(ps => ps.Song)
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s =>
                    s.Status == Status.Active
                    && (s.HostUserId == userId || s.Participants.Any(p => p.UserId == userId))
                );

            if (party == null)
                throw new Exception("User is not in any active party");

            if (party.Playlist == null)
                throw new Exception("Party does not have a playlist");

            // Find the song to add
            var song = await _context.Songs.FirstOrDefaultAsync(s => s.Id == dto.SongId);
            if (song == null)
                throw new Exception("Song not found");

            // Determine the next position in the playlist
            int nextPosition = party.Playlist.Songs.Any()
                ? party.Playlist.Songs.Max(ps => ps.Position) + 1
                : 1;

            // Create the PlaylistSong entry
            var playlistSong = new PlaylistSong
            {
                PlaylistId = party.Playlist.Id,
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

        public async Task<PlaylistSongDTO> RemoveSongFromCurrentParty(int userId, RemoveSongDTO dto)
        {
            // Find the active party the user is currently in (either as host or participant)
            var party = await _context
                .Parties.Include(s => s.Playlist)
                .ThenInclude(p => p.Songs)
                .ThenInclude(ps => ps.Song)
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s =>
                    s.Status == Status.Active
                    && (s.HostUserId == userId || s.Participants.Any(p => p.UserId == userId))
                );

            if (party == null)
                throw new Exception("User is not in any active party");

            if (party.Playlist == null)
                throw new Exception("Party does not have a playlist");

            // Find the playlist entry
            var playlistSong = party.Playlist.Songs.FirstOrDefault(ps => ps.SongId == dto.SongId);

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

        public async Task<PartyDTO?> GetUserActiveParty(int userId)
        {
            // Check if user is hosting an active party
            var hostedParty = await _context
                .Parties.Include(s => s.HostUser)
                .Include(s => s.Participants)
                .ThenInclude(p => p.User)
                .Include(s => s.Playlist)
                .ThenInclude(p => p.Songs)
                .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(s => s.HostUserId == userId && s.Status == Status.Active);

            if (hostedParty != null)
            {
                return MapToPartyDTO(hostedParty);
            }

            // Check if user is participating in an active party
            var participation = await _context
                .Participants.Include(p => p.Party)
                .ThenInclude(s => s.HostUser)
                .Include(p => p.Party)
                .ThenInclude(s => s.Participants)
                .ThenInclude(p => p.User)
                .Include(p => p.Party)
                .ThenInclude(s => s.Playlist)
                .ThenInclude(p => p.Songs)
                .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Party.Status == Status.Active);

            if (participation != null)
            {
                return MapToPartyDTO(participation.Party);
            }

            return null;
        }

        private PartyDTO MapToPartyDTO(Party party)
        {
            return new PartyDTO
            {
                Id = party.Id,
                Name = party.Name,
                Status = party.Status,
                CreatedAt = party.CreatedAt,
                UpdatedAt = party.UpdatedAt,
                CurrentlyPlayingSongId = party.CurrentlyPlayingSongId,
                IsPlaying = party.IsPlaying,
                CurrentPosition = party.CurrentPosition,
                HostUser = new HostUserDTO(party.HostUser.Id, party.HostUser.Username),
                Participants = party
                    .Participants.Select(p => new ParticipantInPartyDTO
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        UserName = p.User.Username,
                        JoinedAt = p.JoinedAt,
                    })
                    .ToList(),
                Playlist =
                    party.Playlist != null
                        ? new PlaylistDTO
                        {
                            Id = party.Playlist.Id,
                            Name = party.Playlist.Name,
                            Songs =
                                party
                                    .Playlist.Songs?.Select(ps => new Dotnet_test.DTOs.Song.SongDTO
                                    {
                                        Id = ps.Song.Id,
                                        Title = ps.Song.Title,
                                        Artist = ps.Song.Artist,
                                        Album = ps.Song.Album,
                                        Duration = ps.Song.Duration,
                                        FilePath = ps.Song.FilePath,
                                    })
                                    .ToList() ?? new List<Dotnet_test.DTOs.Song.SongDTO>(),
                        }
                        : null,
            };
        }
    }
}
