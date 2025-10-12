namespace Dotnet_test.DTOs.Playlist
{
    public class PlaylistSongDTO
    {
        public int Id { get; set; }
        public int SongId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string FilePath { get; set; } = string.Empty;

        public int AddedByUserId { get; set; }
        public int Position { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
