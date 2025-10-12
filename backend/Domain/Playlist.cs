namespace Dotnet_test.Domain
{
    public class Playlist
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // FK to session
        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        // Navigation
        public ICollection<PlaylistSong> Songs { get; set; } = new List<PlaylistSong>();
    }
}
