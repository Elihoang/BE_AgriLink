using System.Security.Claims;

namespace AgriLink_DH.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("id");
        
        if (idClaim != null && Guid.TryParse(idClaim.Value, out var userId))
        {
            return userId;
        }

        throw new InvalidOperationException("Identity claim 'id' or 'NameIdentifier' not found or invalid.");
    }
}
