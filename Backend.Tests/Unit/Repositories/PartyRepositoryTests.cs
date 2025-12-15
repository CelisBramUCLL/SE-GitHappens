using Dotnet_test.Domain;
using Dotnet_test.DTOs.Party;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.Infrastructure;
using Dotnet_test.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnet_test.Tests.Repository
{
    public class PartyRepositoryTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            return context;
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllPartiesWithDetails()
        {
            // Arrange
            var context = GetDbContext();

            var host = new User { Id = 1, Username = "HostUser" };
            context.Users.Add(host);

            var party = new Party
            {
                Id = 1,
                Name = "Party All",
                HostUserId = 1,
                Status = Status.Active,
                CreatedAt = DateTime.UtcNow
            };
            context.Parties.Add(party);

            var playlist = new Playlist { Id = 1, Name = "Main Playlist", PartyId = 1 };
            context.Playlists.Add(playlist);

            var song = new Song
            {
                Id = 1,
                Title = "Track 1",
                Artist = "Artist 1",
                Album = "Album 1",
                Duration = new Duration(3, 0),
                FilePath = "/song.mp3"
            };
            context.Songs.Add(song);

            var ps = new PlaylistSong
            {
                PlaylistId = 1,
                SongId = 1,
                AddedByUserId = 1,
                Position = 1,
                AddedAt = DateTime.UtcNow
            };
            context.PlaylistSongs.Add(ps);

            context.SaveChanges();

            var repo = new PartyRepository(context);

            // Act
            var result = await repo.GetAll();

            // Assert
            Assert.Single(result);
            Assert.Equal("Party All", result[0].Name);
            Assert.Equal("Main Playlist", result[0].Playlist.Name);
        }
        [Fact]
        public async Task GetUserActiveParty_WhenHost_ReturnsParty()
        {
            var context = GetDbContext();

            var host = new User { Id = 1, Username = "HostUser" };
            context.Users.Add(host);

            var party = new Party
            {
                Id = 1,
                Name = "HostParty",
                HostUserId = 1,
                Status = Status.Active,
                CreatedAt = DateTime.UtcNow,
                HostUser = host
            };
            context.Parties.Add(party);
            context.SaveChanges();

            var repo = new PartyRepository(context);

            // Act
            var result = await repo.GetUserActiveParty(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("HostParty", result.Name);
        }

        [Fact]
        public async Task GetUserActiveParty_WhenParticipant_ReturnsParty()
        {
            var context = GetDbContext();

            var host = new User { Id = 1, Username = "HostUser" };
            var guest = new User { Id = 2, Username = "GuestUser" };
            context.Users.AddRange(host, guest);

            var party = new Party
            {
                Id = 1,
                Name = "JoinParty",
                HostUserId = 1,
                Status = Status.Active,
                CreatedAt = DateTime.UtcNow,
                HostUser = host
            };

            var participant = new Participant
            {
                Id = 1,
                PartyId = 1,
                UserId = 2,
                JoinedAt = DateTime.UtcNow
            };

            context.Parties.Add(party);
            context.Participants.Add(participant);
            context.SaveChanges();

            var repo = new PartyRepository(context);

            // Act
            var result = await repo.GetUserActiveParty(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("JoinParty", result.Name);
        }

        [Fact]
        public async Task Create_ShouldAddPartyAndHostParticipant()
        {
            // Arrange
            var context = GetDbContext();

            var user = new User { Id = 1, Username = "Alice" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new PartyRepository(context);
            var newParty = new Party
            {
                Name = "Test Party",
                HostUserId = 1,
                Status = Status.Active,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await repo.Create(newParty);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Party", result.Name);
            Assert.Equal(user.Username, result.HostUser.Username);
            Assert.Single(result.Participants);
        }

        [Fact]
        public async Task JoinParty_ShouldAddParticipant()
        {
            // Arrange
            var context = GetDbContext();

            var host = new User { Id = 1, Username = "HostUser" };
            var user = new User { Id = 2, Username = "Guest" };
            context.Users.AddRange(host, user);

            var party = new Party
            {
                Name = "Joinable",
                HostUserId = 1,
                Status = Status.Active,
                CreatedAt = DateTime.UtcNow
            };
            context.Parties.Add(party);
            await context.SaveChangesAsync();

            var repo = new PartyRepository(context);
            var joinDto = new JoinPartyDTO(party.Id);

            // Act
            var participant = await repo.JoinParty(joinDto, 2);

            // Assert
            Assert.NotNull(participant);
            Assert.Equal(2, participant.UserId);
            Assert.Equal(party.Id, participant.PartyId);
        }

        [Fact]
        public async Task LeaveParty_ShouldRemoveParticipant()
        {
            // Arrange
            var context = GetDbContext();

            var user = new User { Id = 1, Username = "Leaver" };
            var party = new Party
            {
                Id = 1,
                Name = "LeaveParty",
                HostUserId = 2,
                Status = Status.Active,
                CreatedAt = DateTime.UtcNow
            };
            var participant = new Participant
            {
                PartyId = 1,
                UserId = 1,
                JoinedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            context.Parties.Add(party);
            context.Participants.Add(participant);
            await context.SaveChangesAsync();

            var repo = new PartyRepository(context);

            // Act
            var left = await repo.LeaveParty(1, 1);

            // Assert
            Assert.NotNull(left);
            Assert.Equal(1, left.UserId);

            var stillExists = context.Participants.Any(p => p.Id == participant.Id);
            Assert.False(stillExists);
        }
    }
}