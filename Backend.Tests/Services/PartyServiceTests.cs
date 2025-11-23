using Xunit;
using Moq;
using FluentAssertions;
using Dotnet_test.Services;
using Dotnet_test.Hubs;
using Dotnet_test.DTOs.Party;
using Dotnet_test.DTOs.Participant;
using Dotnet_test.DTOs.Playlist;
using Dotnet_test.DTOs.Song;
using Dotnet_test.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Dotnet_test.Domain;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Dotnet_test.DTOs.User;

namespace Backend.Tests.Services
{
    public class PartyServiceTests
    {
        private readonly Mock<IPartyRepository> _repoMock;
        private readonly Mock<IHubContext<PartyHub>> _hubMock;
        private readonly Mock<IHubClients> _clientsMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly PartyService _service;

        public PartyServiceTests()
        {
            _repoMock = new Mock<IPartyRepository>();
            _hubMock = new Mock<IHubContext<PartyHub>>();
            _clientsMock = new Mock<IHubClients>();
            _clientProxyMock = new Mock<IClientProxy>();

            _clientsMock.Setup(c => c.All).Returns(_clientProxyMock.Object);
            _clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
            _hubMock.Setup(h => h.Clients).Returns(_clientsMock.Object);

            _service = new PartyService(_repoMock.Object, _hubMock.Object);
        }

        // ---------- Constructor ----------
        [Fact]
        public void Constructor_ShouldInitialize_Properly()
        {
            var service = new PartyService(_repoMock.Object, _hubMock.Object);
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenAnyDependencyIsNull()
        {
            Action act1 = () => new PartyService(null!, _hubMock.Object);
            Action act2 = () => new PartyService(_repoMock.Object, null!);

            act1.Should().Throw<ArgumentNullException>();
            act2.Should().Throw<ArgumentNullException>();
        }

        // ---------- Simple repository pass-through tests ----------
        [Fact]
        public async Task GetAllAsync_ShouldReturnRepositoryData()
        {
            var parties = new List<PartyDTO> { new() { Id = 1, Name = "Party" } };
            _repoMock.Setup(r => r.GetAll()).ReturnsAsync(parties);

            var result = await _service.GetAllAsync();

            result.Should().BeEquivalentTo(parties);
            _repoMock.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnParty()
        {
            var party = new PartyDTO { Id = 1, Name = "MockParty" };
            _repoMock.Setup(r => r.GetById(1)).ReturnsAsync(party);

            var result = await _service.GetByIdAsync(1);

            result.Should().Be(party);
            _repoMock.Verify(r => r.GetById(1), Times.Once);
        }

        // ---------- Create ----------
        [Fact]
        public async Task CreatePartyAsync_ShouldCallRepository_AndBroadcast()
        {
            var dto = new CreatePartyDTO("Test Party");
            var partyDto = new PartyDTO { Id = 42, Name = dto.Name };
            _repoMock.Setup(r => r.Create(It.IsAny<Party>())).ReturnsAsync(partyDto);

            var result = await _service.CreatePartyAsync(dto, 99);

            result.Should().Be(partyDto);
            _repoMock.Verify(r => r.Create(It.Is<Party>(p =>
                p.Name == dto.Name && p.HostUserId == 99)), Times.Once);
            _clientProxyMock.Verify(c => c.SendCoreAsync(
                "PartyCreated",
                It.Is<object[]>(o => ((PartyDTO)o[0]).Id == 42),
                default
            ), Times.Once);
        }

        // ---------- Update ----------
        [Fact]
        public async Task UpdatePartyAsync_ShouldReturnUpdatedParty()
        {
            var dto = new UpdatePartyDTO { Name = "Updated" };
            var updated = new PartyDTO { Id = 2, Name = "Updated" };
            _repoMock.Setup(r => r.Update(It.IsAny<Party>(), dto)).ReturnsAsync(updated);

            var result = await _service.UpdatePartyAsync(2, dto);

            result.Should().Be(updated);
            _repoMock.Verify(r => r.Update(It.Is<Party>(p => p.Id == 2), dto), Times.Once);
        }

        // ---------- Delete ----------
        [Fact]
        public async Task DeletePartyAsync_ShouldReturnTrue_AndBroadcast_WhenPartyExists()
        {
            var existingParty = new PartyDTO { Id = 5, HostUser = new HostUserDTO(9, "Host") };
            _repoMock.Setup(r => r.GetById(5)).ReturnsAsync(existingParty);
            _repoMock.Setup(r => r.Delete(5)).ReturnsAsync(true);

            var result = await _service.DeletePartyAsync(5, 9);

            result.Should().BeTrue();
            _clientProxyMock.Verify(c => c.SendCoreAsync(
                "PartyDeleted",
                It.IsAny<object[]>(),
                default
            ), Times.Once);
            _clientProxyMock.Verify(c => c.SendCoreAsync(
                "PartyDeletedGlobal",
                It.IsAny<object[]>(),
                default
            ), Times.Once);
        }

        [Fact]
        public async Task DeletePartyAsync_ShouldReturnFalse_WhenPartyDoesNotExist()
        {
            _repoMock.Setup(r => r.GetById(5)).ReturnsAsync((PartyDTO)null);

            var result = await _service.DeletePartyAsync(5, 9);

            result.Should().BeFalse();
            _clientProxyMock.VerifyNoOtherCalls();
        }

        // ---------- Join ----------
        [Fact]
        public async Task JoinPartyAsync_ShouldCallRepo_AndBroadcast()
        {
            var dto = new JoinPartyDTO(1);
            var participant = new ParticipantDTO { Id = 3, PartyId = 1, UserId = 2 };
            _repoMock.Setup(r => r.JoinParty(dto, 2)).ReturnsAsync(participant);

            var result = await _service.JoinPartyAsync(dto, 2);

            result.Should().Be(participant);
            _clientProxyMock.Verify(c => c.SendCoreAsync(
                "UserJoinedParty",
                It.IsAny<object[]>(),
                default
            ), Times.Once);
        }

        // ---------- Leave ----------
        [Fact]
        public async Task LeavePartyAsync_ShouldReturnParticipant_AndBroadcast()
        {
            var participant = new ParticipantInPartyDTO { Id = 1, UserId = 3 };
            _repoMock.Setup(r => r.LeaveParty(4, 3)).ReturnsAsync(participant);

            var result = await _service.LeavePartyAsync(4, 3);

            result.Should().Be(participant);
            _clientProxyMock.Verify(c => c.SendCoreAsync(
                "UserLeftParty",
                It.IsAny<object[]>(),
                default
            ), Times.Once);
        }

        [Fact]
        public async Task LeavePartyAsync_ShouldReturnNull_WhenRepoReturnsNull()
        {
            _repoMock.Setup(r => r.LeaveParty(4, 3)).ReturnsAsync((ParticipantInPartyDTO)null);

            var result = await _service.LeavePartyAsync(4, 3);

            result.Should().BeNull();
        }

        // ---------- Add song ----------
        [Fact]
        public async Task AddSongAsync_ShouldSendHubMessage_WhenUserInParty()
        {
            var dto = new AddSongDTO(12);
            var currentParty = new PartyDTO { Id = 1 };
            var playlistSong = new PlaylistSongDTO { Id = 99 };

            _repoMock.Setup(r => r.GetUserActiveParty(9)).ReturnsAsync(currentParty);
            _repoMock.Setup(r => r.AddSongToCurrentParty(9, dto)).ReturnsAsync(playlistSong);

            var result = await _service.AddSongAsync(dto, 9);

            result.Should().Be(playlistSong);
            _clientProxyMock.Verify(c => c.SendCoreAsync(
                "SongAdded",
                It.IsAny<object[]>(),
                default
            ), Times.Once);
        }

        [Fact]
        public async Task AddSongAsync_ShouldThrow_WhenUserNotInParty()
        {
            var dto = new AddSongDTO(5);
            _repoMock.Setup(r => r.GetUserActiveParty(9))
                .ReturnsAsync((PartyDTO)null);

            Func<Task> act = async () => await _service.AddSongAsync(dto, 9);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*no está en ninguna fiesta activa*");
        }

        // ---------- Remove song ----------
        [Fact]
        public async Task RemoveSongAsync_ShouldSendHub_WhenUserInParty()
        {
            var dto = new RemoveSongDTO(5);
            var currentParty = new PartyDTO { Id = 1 };
            var playlistSong = new PlaylistSongDTO { Id = 100 };
            _repoMock.Setup(r => r.GetUserActiveParty(9)).ReturnsAsync(currentParty);
            _repoMock.Setup(r => r.RemoveSongFromCurrentParty(9, dto))
                .ReturnsAsync(playlistSong);

            var result = await _service.RemoveSongAsync(dto, 9);

            result.Should().Be(playlistSong);
            _clientProxyMock.Verify(c => c.SendCoreAsync(
                "SongRemoved",
                It.IsAny<object[]>(),
                default
            ), Times.Once);
        }

        [Fact]
        public async Task RemoveSongAsync_ShouldThrow_WhenUserNotInParty()
        {
            var dto = new RemoveSongDTO(1);
            _repoMock.Setup(r => r.GetUserActiveParty(10))
                .ReturnsAsync((PartyDTO)null);

            var act = async () => await _service.RemoveSongAsync(dto, 10);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*no está en ninguna fiesta activa*");
        }

        // ---------- Active party ----------
        [Fact]
        public async Task GetMyActivePartyAsync_ShouldReturnParty()
        {
            var active = new PartyDTO { Id = 11, Name = "Active" };
            _repoMock.Setup(r => r.GetUserActiveParty(10)).ReturnsAsync(active);

            var result = await _service.GetMyActivePartyAsync(10);

            result.Should().Be(active);
            _repoMock.Verify(r => r.GetUserActiveParty(10), Times.Once);
        }
    }
}