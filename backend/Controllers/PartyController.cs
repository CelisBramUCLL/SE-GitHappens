using System.Security.Claims;
using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Party;
using Dotnet_test.DTOs.Song;
using Dotnet_test.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartyController : ControllerBase
    {
        private readonly IPartyRepository _partyRepository;

        public PartyController(IPartyRepository partyRepository)
        {
            _partyRepository = partyRepository;
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
            var deleted = await _partyRepository.Delete(id);
            if (!deleted)
                return NotFound($"Party with id {id} not found");
            return Ok($"Party with id {id} deleted");
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

                var playlistSong = await _partyRepository.AddSongToCurrentParty(
                    loggedInUserId,
                    dto
                );
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

                var playlistSong = await _partyRepository.RemoveSongFromCurrentParty(
                    loggedInUserId,
                    dto
                );
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

        // TODO: Add remaining methods (Update, Delete, Join, Leave, AddSong, RemoveSong)
        // These will be implemented as we continue the conversion
    }
}
