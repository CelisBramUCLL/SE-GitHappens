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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dotnet_test.DTOs.User;

namespace Backend.Tests.Unit.Services
{
    public class PartyServiceTests
    {
        private readonly Mock<IPartyRepository> _repoMock;
        private readonly Mock<IHubContext<PartyHub>> _hubMock;
        private readonly Mock<IHubClients> _clientsMock;
        private readonly Mock<IClientProxy> _proxyMock;
        private readonly PartyService _service;

        public PartyServiceTests()
        {
            _repoMock = new Mock<IPartyRepository>();
            _hubMock = new Mock<IHubContext<PartyHub>>();
            _clientsMock = new Mock<IHubClients>();
            _proxyMock = new Mock<IClientProxy>();

            _clientsMock.Setup(c => c.All).Returns(_proxyMock.Object);
            _clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_proxyMock.Object);
            _hubMock.Setup(h => h.Clients).Returns(_clientsMock.Object);

            _service = new PartyService(_repoMock.Object, _hubMock.Object);
        }

        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            var service = new PartyService(_repoMock.Object, _hubMock.Object);
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenAnyDependencyIsNull()
        {
            Action withNullRepo = () => new PartyService(null!, _hubMock.Object);
            Action withNullHub = () => new PartyService(_repoMock.Object, null!);

            withNullRepo.Should().Throw<ArgumentNullException>();
            withNullHub.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnRepositoryData()
        {
            var list = new List<PartyDTO> { new() { Id = 1, Name = "Party" } };
            _repoMock.Setup(r => r.GetAll()).ReturnsAsync(list);

            var result = await _service.GetAllAsync();

            result.Should().BeEquivalentTo(list);
            _repoMock.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnParty()
        {
            var dto = new PartyDTO { Id = 3 };
            _repoMock.Setup(r => r.GetById(3)).ReturnsAsync(dto);

            var result = await _service.GetByIdAsync(3);

            result.Should().Be(dto);
        }

        [Fact]
        public async Task CreatePartyAsync_ShouldCreateAndBroadcast()
        {
            var dto = new CreatePartyDTO("MyParty");
            var resultDto = new PartyDTO { Id = 5, Name = dto.Name };
            _repoMock.Setup(r => r.Create(It.IsAny<Party>())).ReturnsAsync(resultDto);

            var result = await _service.CreatePartyAsync(dto, 7);

            result.Should().Be(resultDto);
            _repoMock.Verify(r => r.Create(It.Is<Party>(p =>
                p.Name == dto.Name && p.HostUserId == 7 &&
                p.Status == Status.Active
            )), Times.Once);

            _proxyMock.Verify(p => p.SendCoreAsync(
                "PartyCreated",
                It.Is<object[]>(o => ((PartyDTO)o[0]).Id == 5),
                default
            ), Times.Once);
        }

        [Fact]
        public async Task CreatePartyAsync_ShouldPropagateException()
        {
            var dto = new CreatePartyDTO("Bad");
            _repoMock.Setup(r => r.Create(It.IsAny<Party>())).ThrowsAsync(new InvalidOperationException("fail"));

            Func<Task> act = async () => await _service.CreatePartyAsync(dto, 9);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("fail");
        }

        [Fact]
        public async Task UpdatePartyAsync_ShouldCallRepositoryAndReturnUpdated()
        {
            var dto = new UpdatePartyDTO { Name = "New" };
            var updated = new PartyDTO { Id = 1, Name = "New" };
            _repoMock.Setup(r => r.Update(It.IsAny<Party>(), dto)).ReturnsAsync(updated);

            var result = await _service.UpdatePartyAsync(1, dto);

            result.Should().Be(updated);
            _repoMock.Verify(r => r.Update(It.Is<Party>(p => p.Id == 1), dto), Times.Once);
        }

        [Fact]
        public async Task DeletePartyAsync_ShouldReturnFalse_WhenPartyMissing()
        {
            _repoMock.Setup(r => r.GetById(5)).ReturnsAsync((PartyDTO?)null);

            var result = await _service.DeletePartyAsync(5, 1);

            result.Should().BeFalse();
            _proxyMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task DeletePartyAsync_ShouldBroadcastAndReturnTrue_WhenExists()
        {
            var party = new PartyDTO { Id = 5, HostUser = new HostUserDTO(3, "Host") };
            _repoMock.Setup(r => r.GetById(5)).ReturnsAsync(party);
            _repoMock.Setup(r => r.Delete(5)).ReturnsAsync(true);

            var result = await _service.DeletePartyAsync(5, 3);

            result.Should().BeTrue();

            _proxyMock.Verify(p => p.SendCoreAsync(
                "PartyDeleted",
                It.IsAny<object[]>(),
                default
            ), Times.Once);

            _proxyMock.Verify(p => p.SendCoreAsync(
                "PartyDeletedGlobal",
                It.IsAny<object[]>(),
                default
            ), Times.Once);
        }

        [Fact]
        public async Task JoinPartyAsync_ShouldCallRepo_AndBroadcast()
        {
            var dto = new JoinPartyDTO(99);
            var participant = new ParticipantDTO { Id = 7 };
            _repoMock.Setup(r => r.JoinParty(dto, 9)).ReturnsAsync(participant);

            var result = await _service.JoinPartyAsync(dto, 9);

            result.Should().Be(participant);
            _proxyMock.Verify(p => p.SendCoreAsync(
                "UserJoinedParty",
                It.IsAny<object[]>(),
                default
            ), Times.Once);
        }

        [Fact]
        public async Task LeavePartyAsync_ShouldBroadcast_WhenFound()
        {
            var participant = new ParticipantInPartyDTO { Id = 1, UserId = 3 };
            _repoMock.Setup(r => r.LeaveParty(5, 3)).ReturnsAsync(participant);

            var result = await _service.LeavePartyAsync(5, 3);

            result.Should().Be(participant);
            _proxyMock.Verify(p => p.SendCoreAsync(
                "UserLeftParty",
                It.IsAny<object[]>(),
                default
            ), Times.Once);
        }

        [Fact]
        public async Task LeavePartyAsync_ShouldReturnNull_WhenRepoReturnsNull()
        {
            _repoMock.Setup(r => r.LeaveParty(4, 1)).ReturnsAsync((ParticipantInPartyDTO?)null);

            var result = await _service.LeavePartyAsync(4, 1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddSongAsync_ShouldThrow_WhenUserNotInParty()
        {
            var dto = new AddSongDTO(1);
            _repoMock.Setup(r => r.GetUserActiveParty(2)).ReturnsAsync((PartyDTO?)null);

            var act = () => _service.AddSongAsync(dto, 2);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*no está en ninguna fiesta activa*");
        }

        [Fact]
        public async Task AddSongAsync_ShouldSendHubNotification_WhenUserInParty()
        {
            var dto = new AddSongDTO(3);
            var party = new PartyDTO { Id = 8 };
            var song = new PlaylistSongDTO { Id = 123 };

            _repoMock.Setup(r => r.GetUserActiveParty(11)).ReturnsAsync(party);
            _repoMock.Setup(r => r.AddSongToCurrentParty(11, dto)).ReturnsAsync(song);

            var result = await _service.AddSongAsync(dto, 11);

            result.Should().Be(song);
            _proxyMock.Verify(p => p.SendCoreAsync("SongAdded", It.IsAny<object[]>(), default), Times.Once);
        }

        [Fact]
        public async Task RemoveSongAsync_ShouldThrow_WhenUserNotInParty()
        {
            var dto = new RemoveSongDTO(3);
            _repoMock.Setup(r => r.GetUserActiveParty(20)).ReturnsAsync((PartyDTO?)null);

            var act = () => _service.RemoveSongAsync(dto, 20);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*no está en ninguna fiesta activa*");
        }

        [Fact]
        public async Task RemoveSongAsync_ShouldWorkAndNotify_WhenUserInParty()
        {
            var dto = new RemoveSongDTO(5);
            var party = new PartyDTO { Id = 4 };
            var song = new PlaylistSongDTO { Id = 9 };

            _repoMock.Setup(r => r.GetUserActiveParty(10)).ReturnsAsync(party);
            _repoMock.Setup(r => r.RemoveSongFromCurrentParty(10, dto)).ReturnsAsync(song);

            var result = await _service.RemoveSongAsync(dto, 10);

            result.Should().Be(song);
            _proxyMock.Verify(p => p.SendCoreAsync("SongRemoved", It.IsAny<object[]>(), default), Times.Once);
        }

        [Fact]
        public async Task GetMyActivePartyAsync_ShouldReturnResult()
        {
            var activeParty = new PartyDTO { Id = 2 };
            _repoMock.Setup(r => r.GetUserActiveParty(9)).ReturnsAsync(activeParty);

            var result = await _service.GetMyActivePartyAsync(9);

            result.Should().Be(activeParty);
        }
    }
}