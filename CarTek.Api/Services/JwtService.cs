using CarTek.Api.Const;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CarTek.Api.Services
{
    public class JwtService : IJwtService
    {
        public string GenerateToken(Claim[] claims, int lifeTimeHours, int lifeTimeMinutes)
        {
            var key = AuthOptions.GetSymmetricSecurityKey();
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(lifeTimeHours).AddSeconds(lifeTimeMinutes),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
