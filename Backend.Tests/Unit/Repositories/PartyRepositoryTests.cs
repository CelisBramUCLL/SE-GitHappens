using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Dotnet_test.Domain;
using Dotnet_test.Infrastructure;
using Dotnet_test.Repository;
using Dotnet_test.DTOs.Party;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Song;
using Dotnet_test.DTOs.Playlist;
using Dotnet_test.DTOs.User;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Backend.Tests.Repository
{
    public class PartyRepositoryTests
    {
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Create_Should_AddPartyPlaylistAndHost()
        {
            await using var context = CreateContext(nameof(Create_Should_AddPartyPlaylistAndHost));
            var user = new User { Username = "host", Email = "a@test.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new PartyRepository(context);
            var now = DateTime.UtcNow;
            var party = new Party
            {
                Name = "My Party",
                HostUserId = user.Id,
                Status = Status.Active,
                CreatedAt = now,
                UpdatedAt = now
            };

            var dto = await repo.Create(party);

            dto.Should().NotBeNull();
            dto.Name.Should().Be("My Party");
            dto.Playlist.Should().NotBeNull();
            dto.Participants.Should().ContainSingle(p => p.UserId == user.Id);

            context.Parties.Count().Should().Be(1);
            context.Playlists.Count().Should().Be(1);
            context.Participants.Count().Should().Be(1);
        }

        [Fact]
        public async Task Create_Should_Throw_WhenUserAlreadyHostingActiveParty()
        {
            await using var context = CreateContext(nameof(Create_Should_Throw_WhenUserAlreadyHostingActiveParty));
            var user = new User { Username = "host2", Email = "b@test.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            context.Parties.Add(new Party
            {
                Name = "Old",
                HostUserId = user.Id,
                Status = Status.Active
            });
            await context.SaveChangesAsync();

            var repo = new PartyRepository(context);
            var newParty = new Party { Name = "New", HostUserId = user.Id, Status = Status.Active };

            Func<Task> act = async () => await repo.Create(newParty);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*hosting an active party*");
        }

        [Fact]
        public async Task Delete_Should_RemoveExistingParty()
        {
            await using var context = CreateContext(nameof(Delete_Should_RemoveExistingParty));
            var user = new User { Username = "x", Email = "x@test.com" };
            var party = new Party { Name = "Party", HostUserId = 1, Status = Status.Active };
            context.Users.Add(user);
            context.Parties.Add(party);
            await context.SaveChangesAsync();

            var repo = new PartyRepository(context);
            var result = await repo.Delete(party.Id);

            result.Should().BeTrue();
            context.Parties.Should().BeEmpty();
        }

        [Fact]
        public async Task Delete_Should_ReturnFalse_WhenMissing()
        {
            await using var context = CreateContext(nameof(Delete_Should_ReturnFalse_WhenMissing));
            var repo = new PartyRepository(context);

            (await repo.Delete(999)).Should().BeFalse();
        }

        [Fact]
        public async Task GetById_Should_Return_PartyDTO()
        {
            await using var context = CreateContext(nameof(GetById_Should_Return_PartyDTO));
            var user = new User { Username = "z", Email = "z@test.com" };
            var party = new Party { Name = "GetParty", HostUser = user, Status = Status.Active };
            context.Users.Add(user);
            context.Parties.Add(party);
            await context.SaveChangesAsync();

            var repo = new PartyRepository(context);
            var dto = await repo.GetById(party.Id);

            dto.Should().NotBeNull();
            dto!.Name.Should().Be("GetParty");
        }

        [Fact]
        public async Task Update_Should_ChangeName_AndReturnDTO()
        {
            await using var context = CreateContext(nameof(Update_Should_ChangeName_AndReturnDTO));
            var user = new User { Username = "hi" };
            var party = new Party { Name = "Old", HostUser = user, Status = Status.Active };
            context.Users.Add(user);
            context.Parties.Add(party);
            await context.SaveChangesAsync();

            var repo = new PartyRepository(context);
            var dto = new UpdatePartyDTO { Name = "NewName" };

            var res = await repo.Update(party, dto);

            res.Should().NotBeNull();
            res!.Name.Should().Be("NewName");
            context.Parties.First().Name.Should().Be("NewName");
        }

        [Fact]
        public async Task JoinParty_Should_AddParticipant_WhenValid()
        {
            await using var context = CreateContext(nameof(JoinParty_Should_AddParticipant_WhenValid));
            var host = new User { Username = "host" };
            var guest = new User { Username = "guest" };
            var party = new Party { Name = "Party", HostUser = host, Status = Status.Active };
            context.AddRange(host, guest, party);
            await context.SaveChangesAsync();

            var repo = new PartyRepository(context);
            var dto = new JoinPartyDTO(party.Id);

            var result = await repo.JoinParty(dto, guest.Id);

            result.PartyId.Should().Be(party.Id);
            context.Participants.Count().Should().Be(1);
        }

        [Fact]
        public async Task JoinParty_Should_Throw_WhenPartyNotFound()
        {
            await using var context = CreateContext(nameof(JoinParty_Should_Throw_WhenPartyNotFound));
            var repo = new PartyRepository(context);
            var dto = new JoinPartyDTO(10);

            Func<Task> act = async () => await repo.JoinParty(dto, 1);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*Party not found*");
        }

        [Fact]
        public async Task LeaveParty_Should_RemoveParticipant()
        {
            await using var context = CreateContext(nameof(LeaveParty_Should_RemoveParticipant));
            var user = new User { Username = "user" };
            var party = new Party { Name = "Leavable", HostUser = user, Status = Status.Active };
            var participant = new Participant
            {
                User = user,
                Party = party,
                JoinedAt = DateTime.UtcNow
            };
            context.AddRange(user, party, participant);
            await context.SaveChangesAsync();

            var repo = new PartyRepository(context);
            var result = await repo.LeaveParty(party.Id, user.Id);

            result.Should().NotBeNull();
            context.Participants.Should().BeEmpty();
        }

        [Fact]
        public async Task AddSongToCurrentParty_Should_AddSongToPlaylist()
        {
            await using var context = CreateContext(nameof(AddSongToCurrentParty_Should_AddSongToPlaylist));
            var user = new User { Username = "u" };
            var party = new Party { Name = "X", HostUser = user, Status = Status.Active };
            var playlist = new Playlist { Name = "pList", Party = party };
            var song = new Song { Title = "Track", Artist = "Artist" };
            context.AddRange(user, party, playlist, song);
            await context.SaveChangesAsync();

            var repo = new PartyRepository(context);
            var dto = new AddSongDTO(song.Id);

            var res = await repo.AddSongToCurrentParty(user.Id, dto);

            res.SongId.Should().Be(song.Id);
            context.PlaylistSongs.Count().Should().Be(1);
        }

        [Fact]
        public async Task RemoveSongFromCurrentParty_Should_RemoveSong()
        {
            await using var context = CreateContext(nameof(RemoveSongFromCurrentParty_Should_RemoveSong));
            var user = new User { Username = "u" };
            var song = new Song { Title = "Track" };
            var playlist = new Playlist { Name = "PL" };
            var party = new Party
            {
                Name = "P",
                HostUser = user,
                Status = Status.Active,
                Playlist = playlist
            };
            var ps = new PlaylistSong { Song = song, Playlist = playlist, AddedByUser = user, Position = 1 };
            playlist.Songs.Add(ps);
            context.AddRange(user, song, party, ps);
            await context.SaveChangesAsync();

            var repo = new PartyRepository(context);
            var dto = new RemoveSongDTO(song.Id);

            var result = await repo.RemoveSongFromCurrentParty(user.Id, dto);

            result.SongId.Should().Be(song.Id);
            context.PlaylistSongs.Should().BeEmpty();
        }
    }
}