using Dotnet_test.Domain;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Session;
using Dotnet_test.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly ISessionRepository _sessionRepository;

        public SessionController(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sessions = await _sessionRepository.GetAll();
            return Ok(sessions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var session = await _sessionRepository.GetById(id);
            if (session == null)
                return NotFound();
            return Ok(session);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSessionDto dto)
        {
            // Get the logged-in user ID from JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            int userId = int.Parse(userIdClaim.Value);

            var session = new Session
            {
                Name = dto.Name,
                Status = Status.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                HostUserId = userId,
            };

            var createdSession = await _sessionRepository.Create(session);
            return CreatedAtAction(nameof(GetById), new { id = createdSession.Id }, createdSession);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSessionDTO dto)
        {
            var session = new Session { Id = id };
            var updated = await _sessionRepository.Update(session, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _sessionRepository.Delete(id);
            if (!deleted)
                return NotFound($"Session with id {id} not found");
            return Ok($"Session with id {id} deleted");
        }

        [Authorize]
        [HttpPost("join")]
        public async Task<IActionResult> JoinSession([FromBody] JoinSessionDTO dto)
        {
            try
            {
                // Get logged-in user ID from JWT
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("User ID not found in token");

                int loggedInUserId = int.Parse(userIdClaim.Value);

                var participant = await _sessionRepository.JoinSession(dto, loggedInUserId);
                return Ok(participant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("leave")]
        public async Task<ActionResult<ParticipantInSessionDTO>> LeaveSession(
            [FromBody] LeaveSessionDTO dto
        )
        {
            // Get logged-in user ID from JWT
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            int loggedInUserId = int.Parse(userIdClaim.Value);

            var participant = await _sessionRepository.LeaveSession(dto.SessionId, loggedInUserId);
            if (participant == null)
                return NotFound(new { message = "Participant not found" });

            return Ok(participant);
        }
    }
}
