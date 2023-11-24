using CarTek.Api.Const;
using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Response;
using Microsoft.IdentityModel.Tokens;
using NPOI.XSSF.UserModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CarTek.Api.Services
{
    public class JwtService : IJwtService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<JwtService> _logger;

        public JwtService(ApplicationDbContext dbContext, ILogger<JwtService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public string GenerateToken(Claim[] claims, int lifeTimeHours, int lifeTimeMinutes)
        {
            var key = AuthOptions.GetSymmetricSecurityKey();
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(3).AddSeconds(lifeTimeMinutes),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<UserAuthResult> Refresh(TokenApiModel tokenApiModel)
        {
            try
            {
                string accessToken = tokenApiModel.AccessToken;
                string refreshToken = tokenApiModel.RefreshToken;
                User identity;
                var principal = GetPrincipalFromExpiredToken(accessToken);
                var username = principal.FindFirst("Login"); //this is mapped to the Name claim by default
                var isDriverClaim = principal.FindFirst("IsDriver"); //this is mapped to the Name claim by default

                if (username != null)
                {
                    if (isDriverClaim != null)
                    {
                        var driver = _dbContext.Drivers.SingleOrDefault(u => u.Login == username.Value);

                        if (driver is null || driver.RefreshToken != refreshToken || driver.RefreshTokenExpiryTime <= DateTime.Now)
                            return null;


                        var newAccessTokenD = GenerateToken(principal.Claims.ToArray(), 0, 1);

                        var newRefreshTokenD = GenerateRefreshToken();
                        driver.RefreshToken = newRefreshTokenD;

                        _dbContext.SaveChanges();

                        return new UserAuthResult
                        {
                            Identity = new User
                            {
                                Id = driver.Id,
                                Login = driver.Login,
                                FirstName = driver.FirstName,
                                LastName = driver.LastName,
                                MiddleName = driver.MiddleName,
                                IsAdmin = false
                            },
                            Token = newAccessTokenD,
                            RefreshToken = newRefreshTokenD,
                            IsDriver = true
                        };
                    }

                    var user = _dbContext.Users.SingleOrDefault(u => u.Login == username.Value);

                    if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                        return null;


                    var newAccessToken = GenerateToken(principal.Claims.ToArray(), 0, 1);
                    var newRefreshToken = GenerateRefreshToken();
                    user.RefreshToken = newRefreshToken;

                    await _dbContext.SaveChangesAsync();

                    return new UserAuthResult
                    {
                        Identity = user,
                        Token = newAccessToken,
                        RefreshToken = newRefreshToken
                    };
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"RefreshToken failed: {ex.Message}", ex.Message);
            };


            return null;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = AuthOptions.GetSymmetricSecurityKey();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            
            SecurityToken securityToken;
            
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
