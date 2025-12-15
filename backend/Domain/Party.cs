namespace Dotnet_test.Domain
{
    public class Party
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Status Status { get; set; } = Status.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Host user (FK)
        public int HostUserId { get; set; }
        public User HostUser { get; set; } = null!;

        // Music playback state
        public int? CurrentlyPlayingSongId { get; set; }
        public bool IsPlaying { get; set; } = false;
        public int CurrentPosition { get; set; } = 0; // Position in seconds

        // Navigation
        public ICollection<Participant> Participants { get; set; } = new List<Participant>();
        public Playlist Playlist { get; set; } = null!;
    }
}
