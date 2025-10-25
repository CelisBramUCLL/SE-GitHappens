using Dotnet_test.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Dotnet_test.Services;

namespace Dotnet_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly ISongService _songService;

        public SongsController(ISongService songService)
        {
            _songService = songService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            var (songs, totalCount, totalPages) = await _songService.GetAllAsync(
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
            var song = await _songService.GetByIdAsync(id);

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

            var songs = await _songService.SearchAsync(query, limit);
            return Ok(songs);
        }
    }
}