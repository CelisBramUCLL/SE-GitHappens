namespace Dotnet_test.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        // Security
        public string PasswordHash { get; set; } = string.Empty;

        public Role Role { get; set; } = Role.User;

        // Navigation
        public ICollection<Party> HostedParties { get; set; } = new List<Party>();
        public ICollection<Participant> PartiesJoined { get; set; } = new List<Participant>();
        public ICollection<PlaylistSong> AddedPlaylistSongs { get; set; } =
            new List<PlaylistSong>();
    }
}
