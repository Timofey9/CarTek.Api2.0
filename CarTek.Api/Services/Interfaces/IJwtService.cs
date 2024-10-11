using CarTek.Api.Model;
using System.Security.Claims;

namespace CarTek.Api.Services
{
    public interface IJwtService
    {
        public string GenerateToken(Claim[] claims, int lifeTimeHours, int lifeTimeMinutes);

        string GenerateRefreshToken();

        Task<UserAuthResult> Refresh(TokenApiModel tokenApiModel);

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
