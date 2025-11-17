using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Party;
using Dotnet_test.DTOs.Song;
using Dotnet_test.DTOs.Playlist;
using Dotnet_test.Hubs;
using Dotnet_test.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet_test.Services
{
    public class PartyService : IPartyService
    {
        private readonly IPartyRepository _partyRepository;
        private readonly IHubContext<PartyHub> _hubContext;

        public PartyService(IPartyRepository partyRepository, IHubContext<PartyHub> hubContext)
        {
            _partyRepository = partyRepository;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<PartyDTO>> GetAllAsync()
        {
            return await _partyRepository.GetAll();
        }

        public async Task<PartyDTO> GetByIdAsync(int id)
        {
            return await _partyRepository.GetById(id);
        }

        public async Task<PartyDTO> CreatePartyAsync(CreatePartyDTO dto, int userId)
        {
            var party = new Party
            {
                Name = dto.Name,
                HostUserId = userId, 
                Status = Status.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            
            try
            {
                var partyDto = await _partyRepository.Create(party);

                await _hubContext.Clients.All.SendAsync("PartyCreated", partyDto);

                return partyDto;
            }
            catch (Exception)
            {
                throw; 
            }
        }

        public async Task<PartyDTO> UpdatePartyAsync(int id, UpdatePartyDTO dto)
        {
            var party = new Party { Id = id };
            return await _partyRepository.Update(party, dto);
        }

        public async Task<bool> DeletePartyAsync(int id, int userId)
        {
            var party = await _partyRepository.GetById(id);
            if (party == null)
                return false; 

            await _hubContext
                .Clients.Group($"Party_{id}")
                .SendAsync("PartyDeleted", id, party.HostUser.Id);

            await _hubContext.Clients.All.SendAsync("PartyDeletedGlobal", id);

            return await _partyRepository.Delete(id);
        }

        public async Task<ParticipantDTO> JoinPartyAsync(JoinPartyDTO dto, int userId)
        {
            var participant = await _partyRepository.JoinParty(dto, userId);

            await _hubContext
                .Clients.Group($"Party_{dto.PartyId}")
                .SendAsync("UserJoinedParty", userId, dto.PartyId);

            return participant;
        }

        public async Task<ParticipantInPartyDTO> LeavePartyAsync(int partyId, int userId)
        {
            var participant = await _partyRepository.LeaveParty(partyId, userId);
            if (participant == null)
                return null;

            await _hubContext
                .Clients.Group($"Party_{partyId}")
                .SendAsync("UserLeftParty", userId, partyId);

            return participant;
        }

        public async Task<PlaylistSongDTO> AddSongAsync(AddSongDTO dto, int userId)
        {
            var currentParty = await _partyRepository.GetUserActiveParty(userId);
            if (currentParty == null)
                throw new Exception("El usuario no está en ninguna fiesta activa");

            var playlistSong = await _partyRepository.AddSongToCurrentParty(userId, dto);

            await _hubContext
                .Clients.Group($"Party_{currentParty.Id}")
                .SendAsync("SongAdded", dto.SongId, "server");

            return playlistSong;
        }

        public async Task<PlaylistSongDTO> RemoveSongAsync(RemoveSongDTO dto, int userId)
        {
            var currentParty = await _partyRepository.GetUserActiveParty(userId);
            if (currentParty == null)
                throw new Exception("El usuario no está en ninguna fiesta activa");

            var playlistSong = await _partyRepository.RemoveSongFromCurrentParty(userId, dto);

            await _hubContext
                .Clients.Group($"Party_{currentParty.Id}")
                .SendAsync("SongRemoved", dto.SongId, "server");

            return playlistSong;
        }

        public async Task<PartyDTO> GetMyActivePartyAsync(int userId)
        {
            return await _partyRepository.GetUserActiveParty(userId);
        }
    }
}
