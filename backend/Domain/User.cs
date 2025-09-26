namespace Dotnet_test.Domain
{
    public enum Role
    {
        Admin,
        User,
        Guest,
    }

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
        public ICollection<Session> HostedSessions { get; set; } = new List<Session>();
        public ICollection<Participant> SessionsJoined { get; set; } = new List<Participant>();
        public ICollection<PlaylistSong> AddedPlaylistSongs { get; set; } =
            new List<PlaylistSong>();
    }
}
