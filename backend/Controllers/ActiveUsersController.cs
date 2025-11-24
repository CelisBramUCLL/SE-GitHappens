using Dotnet_test.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActiveUsersController : ControllerBase
    {
        private readonly IActiveUserService _activeUserService;

        public ActiveUsersController(IActiveUserService activeUserService)
        {
            _activeUserService =
                activeUserService ?? throw new ArgumentNullException(nameof(activeUserService));
        }

        /// <summary>
        /// Get the count of currently active users
        /// </summary>
        [HttpGet("count")]
        public IActionResult GetActiveUserCount()
        {
            var count = _activeUserService.GetActiveUserCount();
            return Ok(new { activeUserCount = count });
        }

        /// <summary>
        /// Get all active user IDs (requires authentication)
        /// </summary>
        [HttpGet("users")]
        public IActionResult GetActiveUsers()
        {
            var activeUserIds = _activeUserService.GetActiveUserIds();
            return Ok(new { activeUsers = activeUserIds, count = activeUserIds.Count() });
        }

        /// <summary>
        /// Check if a specific user is active
        /// </summary>
        [HttpGet("users/{userId}/status")]
        public IActionResult CheckUserStatus(int userId)
        {
            var isActive = _activeUserService.IsUserActive(userId);
            return Ok(new { userId = userId, isActive = isActive });
        }
    }
}
