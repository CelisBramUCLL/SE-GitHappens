using Dotnet_test.DTOs.Song;

namespace Dotnet_test.Interfaces
{
    public interface ISongRepository
    {
        Task<(List<SongDTO> songs, int totalCount, int totalPages)> GetAll(
            string? search = null,
            int page = 1,
            int pageSize = 20
        );
        Task<SongDTO?> GetById(int id);
        Task<List<SongDTO>> Search(string query, int limit = 10);
    }
}
