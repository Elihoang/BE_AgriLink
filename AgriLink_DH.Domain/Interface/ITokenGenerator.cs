using System.Security.Claims;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface;

public interface ITokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    ClaimsPrincipal? ValidateToken(string token);
}
