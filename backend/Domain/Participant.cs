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

        public int PartyId { get; set; }
        public Party Party { get; set; } = null!;

        // Role in the party
        public ParticipantRole Role { get; set; } = ParticipantRole.Member;

        // Lifecycle
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }
    }
}
