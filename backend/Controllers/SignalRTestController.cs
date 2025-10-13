using Dotnet_test.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet_test.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignalRTestController : ControllerBase
    {
        private readonly IHubContext<PartyHub> _hubContext;

        public SignalRTestController(IHubContext<PartyHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("test-join/{partyId}")]
        public async Task<IActionResult> TestJoinParty(int partyId)
        {
            try
            {
                // Send test event without authentication
                await _hubContext
                    .Clients.Group($"Party_{partyId}")
                    .SendAsync("UserJoinedParty", 999, partyId);

                return Ok(new { message = $"Test join event sent for party {partyId}" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("test-leave/{partyId}")]
        public async Task<IActionResult> TestLeaveParty(int partyId)
        {
            try
            {
                // Send test event without authentication
                await _hubContext
                    .Clients.Group($"Party_{partyId}")
                    .SendAsync("UserLeftParty", 999, partyId);

                return Ok(new { message = $"Test leave event sent for party {partyId}" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
