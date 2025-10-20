using Dotnet_test.Domain;
using Dotnet_test.DTOs.User;

namespace Dotnet_test.DTOs.Song
{
    public class SongDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public Duration Duration { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public HostUserDTO? AddedBy { get; set; } = null;
    }
}
