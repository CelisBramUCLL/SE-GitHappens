using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Party;
using Dotnet_test.DTOs.Song;
using Dotnet_test.Extensions;
using Dotnet_test.Interfaces; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartyController : ControllerBase
    {
        private readonly IPartyService _partyService;

        public PartyController(IPartyService partyService)
        {
            _partyService = partyService ?? throw new ArgumentNullException(nameof(partyService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var parties = await _partyService.GetAllAsync();
            return Ok(parties);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var party = await _partyService.GetByIdAsync(id); 
            if (party == null)
                return NotFound();
            return Ok(party);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateParty([FromBody] CreatePartyDTO dto)
        {
            int? userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized("User ID not found in token");

            try
            {
                var partyDto = await _partyService.CreatePartyAsync(dto, userId.Value);

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
            var updated = await _partyService.UpdatePartyAsync(id, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int? userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized("User ID not found in token");
                
            var deleted = await _partyService.DeletePartyAsync(id, userId.Value);
            
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
                int? loggedInUserId = User.GetUserId();
                if (!loggedInUserId.HasValue)
                    return Unauthorized("User ID not found in token");

                var participant = await _partyService.JoinPartyAsync(dto, loggedInUserId.Value);

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
                int? loggedInUserId = User.GetUserId();
                if (!loggedInUserId.HasValue)
                    return Unauthorized("User ID not found in token");

                var participant = await _partyService.LeavePartyAsync(id, loggedInUserId.Value);
                
                if (participant == null)
                    return NotFound("Participant not found or user not in this party");

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
                int? loggedInUserId = User.GetUserId();
                if (!loggedInUserId.HasValue)
                    return Unauthorized("User ID not found in token");

                var playlistSong = await _partyService.AddSongAsync(dto, loggedInUserId.Value);

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
                int? loggedInUserId = User.GetUserId();
                if (!loggedInUserId.HasValue)
                    return Unauthorized("User ID not found in token");

                var playlistSong = await _partyService.RemoveSongAsync(dto, loggedInUserId.Value);

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
                int? loggedInUserId = User.GetUserId();
                if (!loggedInUserId.HasValue)
                    return Unauthorized("User ID not found in token");

                var activeParty = await _partyService.GetMyActivePartyAsync(loggedInUserId.Value);

                if (activeParty == null)
                    return Ok(new { hasActiveParty = false, party = (object?)null });

                return Ok(new { hasActiveParty = true, party = activeParty });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}