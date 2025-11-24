using Dotnet_test.Extensions;
using Dotnet_test.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet_test.Hubs
{
    [Authorize]
    public class PartyHub : Hub
    {
        private readonly IActiveUserService _activeUserService;
        private readonly ILogger<PartyHub> _logger;

        public PartyHub(IActiveUserService activeUserService, ILogger<PartyHub> logger)
        {
            _activeUserService =
                activeUserService ?? throw new ArgumentNullException(nameof(activeUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.GetUserId();
            if (userId.HasValue)
            {
                _activeUserService.AddUser(userId.Value, Context.ConnectionId);

                // Notify all clients about the updated user count
                await Clients.All.SendAsync(
                    "ActiveUserCountUpdated",
                    _activeUserService.GetActiveUserCount()
                );

                _logger.LogInformation(
                    "User {UserId} connected. Total active users: {Count}",
                    userId.Value,
                    _activeUserService.GetActiveUserCount()
                );
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var wasRemoved = _activeUserService.RemoveUserByConnection(Context.ConnectionId);

            if (wasRemoved)
            {
                // Notify all clients about the updated user count
                await Clients.All.SendAsync(
                    "ActiveUserCountUpdated",
                    _activeUserService.GetActiveUserCount()
                );

                _logger.LogInformation(
                    "User disconnected. Total active users: {Count}",
                    _activeUserService.GetActiveUserCount()
                );
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Join a specific party group
        public async Task JoinParty(int partyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Party_{partyId}");
            await Clients
                .Group($"Party_{partyId}")
                .SendAsync("UserJoinedParty", Context.ConnectionId, partyId);
        }

        // Leave a specific party group
        public async Task LeaveParty(int partyId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Party_{partyId}");
            await Clients
                .Group($"Party_{partyId}")
                .SendAsync("UserLeftParty", Context.ConnectionId, partyId);
        }

        // Notify when a song is added
        public async Task NotifySongAdded(int partyId, int songId)
        {
            await Clients
                .Group($"Party_{partyId}")
                .SendAsync("SongAdded", songId, Context.ConnectionId);
        }

        // Notify when a song is removed
        public async Task NotifySongRemoved(int partyId, int songId)
        {
            await Clients
                .Group($"Party_{partyId}")
                .SendAsync("SongRemoved", songId, Context.ConnectionId);
        }
    }
}
