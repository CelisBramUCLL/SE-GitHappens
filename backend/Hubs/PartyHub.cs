using Microsoft.AspNetCore.SignalR;

namespace Dotnet_test.Hubs
{
    public class PartyHub : Hub
    {
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
