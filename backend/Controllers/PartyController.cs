using System.Security.Claims;
using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Party;
using Dotnet_test.DTOs.Song;
using Dotnet_test.Hubs;
using Dotnet_test.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartyController : ControllerBase
    {
        private readonly IPartyRepository _partyRepository;
        private readonly IHubContext<PartyHub> _hubContext;

        public PartyController(IPartyRepository partyRepository, IHubContext<PartyHub> hubContext)
        {
            _partyRepository = partyRepository;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var parties = await _partyRepository.GetAll();
            return Ok(parties);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var party = await _partyRepository.GetById(id);
            if (party == null)
                return NotFound();
            return Ok(party);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePartyDto dto)
        {
            // Get logged-in user ID from JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            int userId = int.Parse(userIdClaim.Value);

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

                // Notify all users of new party
                await _hubContext.Clients.All.SendAsync("PartyCreated", partyDto);

                return CreatedAtAction(nameof(GetById), new { id = partyDto.Id }, partyDto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating party: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePartyDTO dto)
        {
            var party = new Party { Id = id };
            var updated = await _partyRepository.Update(party, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Get logged-in user ID from JWT token (the host)
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            int hostUserId = int.Parse(userIdClaim.Value);

            // First check if the party exists
            var party = await _partyRepository.GetById(id);
            if (party == null)
                return NotFound(new { error = $"Party with id {id} not found" });

            // Notify party members
            await _hubContext
                .Clients.Group($"Party_{id}")
                .SendAsync("PartyDeleted", id, hostUserId);

            // Notify all users for dashboard updates
            await _hubContext.Clients.All.SendAsync("PartyDeletedGlobal", id);

            var deleted = await _partyRepository.Delete(id);
            if (!deleted)
                return NotFound(new { error = $"Party with id {id} not found" });

            return Ok(new { message = $"Party with id {id} deleted successfully", success = true });
        }

        [Authorize]
        [HttpPost("join")]
        public async Task<IActionResult> JoinParty([FromBody] JoinPartyDTO dto)
        {
            try
            {
                // Get logged-in user ID from JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("User ID not found in token");

                int loggedInUserId = int.Parse(userIdClaim.Value);

                var participant = await _partyRepository.JoinParty(dto, loggedInUserId);

                // Notify party members of new participant
                await _hubContext
                    .Clients.Group($"Party_{dto.PartyId}")
                    .SendAsync("UserJoinedParty", loggedInUserId, dto.PartyId);

                return Ok(participant);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("leave/{id}")]
        public async Task<IActionResult> LeaveParty(int id)
        {
            try
            {
                // Get logged-in user ID from JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("User ID not found in token");

                int loggedInUserId = int.Parse(userIdClaim.Value);

                var participant = await _partyRepository.LeaveParty(id, loggedInUserId);
                if (participant == null)
                    return NotFound("Participant not found or user not in this party");

                // Notify party members of participant leaving
                await _hubContext
                    .Clients.Group($"Party_{id}")
                    .SendAsync("UserLeftParty", loggedInUserId, id);

                return Ok(participant);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("add-song")]
        public async Task<IActionResult> AddSong([FromBody] AddSongDTO dto)
        {
            try
            {
                // Get logged-in user ID from JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("User ID not found in token");

                int loggedInUserId = int.Parse(userIdClaim.Value);

                // Get current party for notifications
                var currentParty = await _partyRepository.GetUserActiveParty(loggedInUserId);
                if (currentParty == null)
                    return BadRequest("User is not in any active party");

                var playlistSong = await _partyRepository.AddSongToCurrentParty(
                    loggedInUserId,
                    dto
                );

                // Notify party members of song addition
                await _hubContext
                    .Clients.Group($"Party_{currentParty.Id}")
                    .SendAsync("SongAdded", dto.SongId, "server");

                return Ok(playlistSong);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("remove-song")]
        public async Task<IActionResult> RemoveSong([FromBody] RemoveSongDTO dto)
        {
            try
            {
                // Get logged-in user ID from JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("User ID not found in token");

                int loggedInUserId = int.Parse(userIdClaim.Value);

                // Get current party for notifications
                var currentParty = await _partyRepository.GetUserActiveParty(loggedInUserId);
                if (currentParty == null)
                    return BadRequest("User is not in any active party");

                var playlistSong = await _partyRepository.RemoveSongFromCurrentParty(
                    loggedInUserId,
                    dto
                );

                // Notify party members of song removal
                await _hubContext
                    .Clients.Group($"Party_{currentParty.Id}")
                    .SendAsync("SongRemoved", dto.SongId, "server");

                return Ok(playlistSong);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("my-active-party")]
        public async Task<IActionResult> GetMyActiveParty()
        {
            try
            {
                // Get logged-in user ID from JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("User ID not found in token");

                int loggedInUserId = int.Parse(userIdClaim.Value);

                var activeParty = await _partyRepository.GetUserActiveParty(loggedInUserId);

                if (activeParty == null)
                    return Ok(new { hasActiveParty = false, party = (object)null });

                return Ok(new { hasActiveParty = true, party = activeParty });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // These will be implemented as we continue the conversion
    }
}
