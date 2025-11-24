using Xunit;
using Moq;
using Microsoft.AspNetCore.SignalR;
using Dotnet_test.Hubs;
using Dotnet_test.Interfaces;
using Microsoft.Extensions.Logging;

namespace Backend.Tests.Hubs
{
    public class PartyHubTests
    {
        private readonly PartyHub _hub;
        private readonly Mock<IHubCallerClients> _clientsMock;
        private readonly Mock<IGroupManager> _groupsMock;
        private readonly Mock<IClientProxy> _groupProxyMock;
        private readonly Mock<IActiveUserService> _activeUserServiceMock;
        private readonly Mock<ILogger<PartyHub>> _loggerMock;
        private readonly HubCallerContext _context;

        public PartyHubTests()
        {
            _clientsMock = new Mock<IHubCallerClients>();
            _groupsMock = new Mock<IGroupManager>();
            _groupProxyMock = new Mock<IClientProxy>();
            _activeUserServiceMock = new Mock<IActiveUserService>();
            _loggerMock = new Mock<ILogger<PartyHub>>();

            _clientsMock.Setup(c => c.Group(It.IsAny<string>()))
                        .Returns(_groupProxyMock.Object);

            var ctx = new Mock<HubCallerContext>();
            ctx.Setup(c => c.ConnectionId).Returns("conn-1");
            _context = ctx.Object;

            // Pass the required dependencies to the constructor
            _hub = new PartyHub(_activeUserServiceMock.Object, _loggerMock.Object)
            {
                Clients = _clientsMock.Object,
                Groups = _groupsMock.Object,
                Context = _context
            };
        }

        [Fact]
        public async Task JoinParty_ShouldAddUserToGroup_AndNotifyGroup()
        {
            await _hub.JoinParty(5);

            _groupsMock.Verify(g => g.AddToGroupAsync("conn-1", "Party_5", default), Times.Once);
            _groupProxyMock.Verify(c => c.SendCoreAsync(
                "UserJoinedParty",
                It.Is<object[]>(a => (string)a[0] == "conn-1" && (int)a[1] == 5),
                default
            ), Times.Once);
        }

        [Fact]
        public async Task LeaveParty_ShouldRemoveUserFromGroup_AndNotifyGroup()
        {
            await _hub.LeaveParty(3);

            _groupsMock.Verify(g => g.RemoveFromGroupAsync("conn-1", "Party_3", default), Times.Once);
            _groupProxyMock.Verify(c => c.SendCoreAsync(
                "UserLeftParty",
                It.Is<object[]>(a => (string)a[0] == "conn-1" && (int)a[1] == 3),
                default
            ), Times.Once);
        }

        [Fact]
        public async Task NotifySongAdded_ShouldBroadcastToGroup()
        {
            await _hub.NotifySongAdded(7, 99);

            _groupProxyMock.Verify(c => c.SendCoreAsync(
                "SongAdded",
                It.Is<object[]>(a => (int)a[0] == 99 && (string)a[1] == "conn-1"),
                default
            ), Times.Once);
        }

        [Fact]
        public async Task NotifySongRemoved_ShouldBroadcastToGroup()
        {
            await _hub.NotifySongRemoved(9, 101);

            _groupProxyMock.Verify(c => c.SendCoreAsync(
                "SongRemoved",
                It.Is<object[]>(a => (int)a[0] == 101 && (string)a[1] == "conn-1"),
                default
            ), Times.Once);
        }
    }
}