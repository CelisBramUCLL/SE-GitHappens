using Dotnet_test.Domain;
using Dotnet_test.Interfaces;
using Dotnet_test.DTOs.Song;


namespace Dotnet_test.Services
{
    public class SongService : ISongService
    {
        private readonly ISongRepository _songRepository;

        public SongService(ISongRepository songRepository)
        {
            _songRepository = songRepository;
        }

        public async Task<(IEnumerable<SongDTO> songs, int totalCount, int totalPages)> GetAllAsync(
            string? search, int page, int pageSize
        )
        {
            return await _songRepository.GetAll(search, page, pageSize);
        }

        public async Task<SongDTO> GetByIdAsync(int id)
        {
            return await _songRepository.GetById(id);
        }

        public async Task<IEnumerable<SongDTO>> SearchAsync(string query, int limit)
        {
            return await _songRepository.Search(query, limit);
        }
    }
}