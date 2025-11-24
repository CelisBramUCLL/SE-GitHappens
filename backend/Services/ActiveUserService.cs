using System.Collections.Concurrent;
using Dotnet_test.Interfaces;

namespace Dotnet_test.Services
{
    /// <summary>
    /// Thread-safe service for tracking active users using ConcurrentDictionary
    /// </summary>
    public class ActiveUserService : IActiveUserService
    {
        // Primary mapping: UserId -> ConnectionId
        private readonly ConcurrentDictionary<int, string> _userToConnection = new();

        // Reverse mapping: ConnectionId -> UserId (for efficient lookups)
        private readonly ConcurrentDictionary<string, int> _connectionToUser = new();

        private readonly ILogger<ActiveUserService> _logger;

        public ActiveUserService(ILogger<ActiveUserService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddUser(int userId, string connectionId)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(connectionId))
            {
                _logger.LogWarning(
                    "Invalid parameters: UserId={UserId}, ConnectionId={ConnectionId}",
                    userId,
                    connectionId
                );
                return;
            }

            // Remove any existing connection for this user (handles reconnections)
            if (_userToConnection.TryGetValue(userId, out var existingConnectionId))
            {
                _connectionToUser.TryRemove(existingConnectionId, out _);
            }

            // Add the new mappings atomically
            _userToConnection.AddOrUpdate(userId, connectionId, (key, oldValue) => connectionId);
            _connectionToUser.AddOrUpdate(connectionId, userId, (key, oldValue) => userId);

            _logger.LogInformation(
                "User {UserId} connected with connection {ConnectionId}. Total active users: {Count}",
                userId,
                connectionId,
                _userToConnection.Count
            );
        }

        public bool RemoveUserByConnection(string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                return false;

            // Remove from both dictionaries atomically
            if (_connectionToUser.TryRemove(connectionId, out var userId))
            {
                _userToConnection.TryRemove(userId, out _);

                _logger.LogInformation(
                    "User {UserId} disconnected (connection {ConnectionId}). Total active users: {Count}",
                    userId,
                    connectionId,
                    _userToConnection.Count
                );

                return true;
            }

            return false;
        }

        public bool RemoveUserById(int userId)
        {
            if (userId <= 0)
                return false;

            // Remove from both dictionaries atomically
            if (_userToConnection.TryRemove(userId, out var connectionId))
            {
                _connectionToUser.TryRemove(connectionId, out _);

                _logger.LogInformation(
                    "User {UserId} removed. Total active users: {Count}",
                    userId,
                    _userToConnection.Count
                );

                return true;
            }

            return false;
        }

        public int GetActiveUserCount()
        {
            return _userToConnection.Count;
        }

        public IEnumerable<int> GetActiveUserIds()
        {
            // Return a snapshot to avoid enumeration issues
            return _userToConnection.Keys.ToArray();
        }

        public bool IsUserActive(int userId)
        {
            return _userToConnection.ContainsKey(userId);
        }

        public string? GetConnectionId(int userId)
        {
            _userToConnection.TryGetValue(userId, out var connectionId);
            return connectionId;
        }
    }
}
