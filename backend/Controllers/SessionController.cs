using Dotnet_test.Domain;
using Dotnet_test.DTOs.Session;
using Dotnet_test.Interfaces;
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSessionDto dto)
        {
            // TODO: Replace with actual userId from authentication
            int userId = 4;

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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSessionDTO dto)
        {
            var session = new Session { Id = id };
            var updated = await _sessionRepository.Update(session, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _sessionRepository.Delete(id);
            if (!deleted)
                return NotFound($"Session with id {id} not found");
            return Ok($"Session with id {id} deleted");
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinSession([FromBody] JoinSessionDTO dto)
        {
            try
            {
                var participant = await _sessionRepository.JoinSession(dto);
                return Ok(participant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
