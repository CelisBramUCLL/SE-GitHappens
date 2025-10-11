using Dotnet_test.DTOs.Song;

namespace Dotnet_test.DTOs.Playlist
{
    public class PlaylistDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<SongDTO> Songs { get; set; } = new List<SongDTO>();
    }
}
