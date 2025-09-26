namespace Dotnet_test.DTOs.Playlist
{
    public class PlaylistSongDTO
    {
        public int Id { get; set; } // PlaylistSong entry ID
        public int SongId { get; set; } // Actual Song ID
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string FilePath { get; set; } = string.Empty;

        public int AddedByUserId { get; set; } // User who added the song
        public int Position { get; set; } // Position in the playlist
        public DateTime AddedAt { get; set; } // Timestamp of when added
    }
}
