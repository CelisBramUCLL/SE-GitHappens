namespace Dotnet_test.Domain
{
    public enum ParticipantRole
    {
        Host,
        Member,
    }

    public class Participant
    {
        public int Id { get; set; }

        // Foreign Keys
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        // Role in the session
        public ParticipantRole Role { get; set; } = ParticipantRole.Member;

        // Lifecycle
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }
    }
}
