namespace Dotnet_test.Domain
{
    public class Song
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public Duration Duration { get; set; }

        // Where the file is stored (could be local path or cloud URL)
        public string FilePath { get; set; } = string.Empty;

        // Navigation
        public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    }
}
