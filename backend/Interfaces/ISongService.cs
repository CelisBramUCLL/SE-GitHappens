using Dotnet_test.Domain; 
using Dotnet_test.DTOs.Song;

namespace Dotnet_test.Interfaces
{
    public interface ISongService
    {
        Task<(IEnumerable<SongDTO> songs, int totalCount, int totalPages)> GetAllAsync(
            string? search, 
            int page, 
            int pageSize
        );

        Task<SongDTO> GetByIdAsync(int id);

        Task<IEnumerable<SongDTO>> SearchAsync(string query, int limit);
    }
}