namespace Dotnet_test.Domain
{
    public class PlaylistSong
    {
        public int Id { get; set; }

        // FK to Playlist
        public int PlaylistId { get; set; }
        public Playlist Playlist { get; set; } = null!;

        // FK to Song
        public int SongId { get; set; }
        public Song Song { get; set; } = null!;

        // Who added it
        public int AddedByUserId { get; set; }
        public User AddedByUser { get; set; } = null!;

        // Queue/order info
        public int Position { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
