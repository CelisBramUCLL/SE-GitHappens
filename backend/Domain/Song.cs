namespace Dotnet_test.Domain
{
    public class Song : IComparable<Song>
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

        #region IComparable<Song> Implementation
        // Compares songs for sorting. Orders by Title first, then by Artist.
        public int CompareTo(Song? other)
        {
            if (other == null)
                return 1;

            // First compare by Title
            var titleComparison = string.Compare(
                Title,
                other.Title,
                StringComparison.OrdinalIgnoreCase
            );
            if (titleComparison != 0)
                return titleComparison;

            // If titles are the same, compare by Artist
            return string.Compare(Artist, other.Artist, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
