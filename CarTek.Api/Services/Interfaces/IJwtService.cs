using System.Security.Claims;

namespace CarTek.Api.Services
{
    public interface IJwtService
    {
        public string GenerateToken(Claim[] claims, int lifeTimeHours, int lifeTimeMinutes);
    }
}
