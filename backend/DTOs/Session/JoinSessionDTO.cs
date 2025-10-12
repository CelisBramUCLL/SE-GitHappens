namespace Dotnet_test.DTOs.Session
{
    public class JoinSessionDTO
    {
        public int SessionId { get; set; }
        public int UserId { get; set; } // in real app, usually from JWT claims
    }
}
