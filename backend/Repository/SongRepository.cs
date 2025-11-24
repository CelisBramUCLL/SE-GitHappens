using Dotnet_test.DTOs.Song;
using Dotnet_test.Infrastructure;
using Dotnet_test.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dotnet_test.Repository
{
    public class SongRepository : GenericRepository<Domain.Song, int>, ISongRepository
    {
        public SongRepository(ApplicationDbContext context)
            : base(context) { }

        public async Task<(List<SongDTO> songs, int totalCount, int totalPages)> GetAll(
            string? search = null,
            int page = 1,
            int pageSize = 20
        )
        {
            var query = _context.Songs.AsQueryable();

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(s =>
                    s.Title.ToLower().Contains(search)
                    || s.Artist.ToLower().Contains(search)
                    || s.Album.ToLower().Contains(search)
                );
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Get paginated results - ordered by Title, then Artist (matches IComparable implementation)
            var songs = await query
                .OrderBy(s => s.Title)
                .ThenBy(s => s.Artist)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SongDTO
                {
                    Id = s.Id,
                    Title = s.Title,
                    Artist = s.Artist,
                    Album = s.Album,
                    Duration = s.Duration,
                    FilePath = s.FilePath,
                })
                .ToListAsync();

            return (songs, totalCount, totalPages);
        }

        public async Task<SongDTO?> GetById(int id)
        {
            // Use inherited generic method instead of manual query
            var song = await GetByIdAsync(id);

            if (song == null)
                return null;

            return new SongDTO
            {
                Id = song.Id,
                Title = song.Title,
                Artist = song.Artist,
                Album = song.Album,
                Duration = song.Duration,
                FilePath = song.FilePath,
            };
        }

        public async Task<List<SongDTO>> Search(string query, int limit = 10)
        {
            if (string.IsNullOrEmpty(query))
                return new List<SongDTO>();

            query = query.ToLower();
            var songs = await _context
                .Songs.Where(s =>
                    s.Title.ToLower().Contains(query)
                    || s.Artist.ToLower().Contains(query)
                    || s.Album.ToLower().Contains(query)
                )
                .OrderBy(s => s.Title)
                .ThenBy(s => s.Artist)
                .Take(limit)
                .Select(s => new SongDTO
                {
                    Id = s.Id,
                    Title = s.Title,
                    Artist = s.Artist,
                    Album = s.Album,
                    Duration = s.Duration,
                    FilePath = s.FilePath,
                })
                .ToListAsync();

            return songs;
        }
    }
}
