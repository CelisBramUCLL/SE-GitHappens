using System.Collections.Concurrent;

namespace Dotnet_test.Interfaces
{
    public interface IActiveUserService
    {
        void AddUser(int userId, string connectionId);

        bool RemoveUserByConnection(string connectionId);

        bool RemoveUserById(int userId);

        int GetActiveUserCount();

        IEnumerable<int> GetActiveUserIds();

        bool IsUserActive(int userId);

        string? GetConnectionId(int userId);
    }
}
