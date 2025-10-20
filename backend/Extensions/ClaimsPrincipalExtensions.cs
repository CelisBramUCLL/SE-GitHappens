using System.Security.Claims;

namespace Dotnet_test.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null)
                return null;

            return int.TryParse(idClaim.Value, out var id) ? id : null;
        }
    }
}
