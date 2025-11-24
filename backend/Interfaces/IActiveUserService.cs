using System.Collections.Concurrent;

namespace Dotnet_test.Interfaces
{
    /// <summary>
    /// Service for tracking active users using concurrent collections
    /// </summary>
    public interface IActiveUserService
    {
        /// <summary>
        /// Add or update an active user with their connection ID
        /// </summary>
        void AddUser(int userId, string connectionId);

        /// <summary>
        /// Remove a user by their connection ID
        /// </summary>
        bool RemoveUserByConnection(string connectionId);

        /// <summary>
        /// Remove a user by their user ID
        /// </summary>
        bool RemoveUserById(int userId);

        /// <summary>
        /// Get the count of currently active users
        /// </summary>
        int GetActiveUserCount();

        /// <summary>
        /// Get all active user IDs
        /// </summary>
        IEnumerable<int> GetActiveUserIds();

        /// <summary>
        /// Check if a user is currently active
        /// </summary>
        bool IsUserActive(int userId);

        /// <summary>
        /// Get the connection ID for a specific user
        /// </summary>
        string? GetConnectionId(int userId);
    }
}
