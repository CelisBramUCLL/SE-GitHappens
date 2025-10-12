using Dotnet_test.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly ISongRepository _songRepository;

        public SongsController(ISongRepository songRepository)
        {
            _songRepository = songRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            var (songs, totalCount, totalPages) = await _songRepository.GetAll(
                search,
                page,
                pageSize
            );

            return Ok(
                new
                {
                    songs,
                    totalCount,
                    page,
                    pageSize,
                    totalPages,
                }
            );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var song = await _songRepository.GetById(id);

            if (song == null)
                return NotFound();

            return Ok(song);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] int limit = 10
        )
        {
            if (string.IsNullOrEmpty(query))
                return BadRequest("Search query cannot be empty");

            var songs = await _songRepository.Search(query, limit);
            return Ok(songs);
        }
    }
}
