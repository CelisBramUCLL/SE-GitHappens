using Dotnet_test.Domain; 

namespace Dotnet_test.Interfaces
{
    public interface ISongService
    {
        Task<(IEnumerable<Song> songs, int totalCount, int totalPages)> GetAllAsync(
            string? search, 
            int page, 
            int pageSize
        );

        Task<Song> GetByIdAsync(int id);

        Task<IEnumerable<Song>> SearchAsync(string query, int limit);
    }
}