using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoreReserve.Core.AppSettings;
using CoreReserve.Core.SharedKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CoreReserve.Infrastructure.Security.Services
{
    internal sealed class JwtTokenService : IJwtTokenService
    {
        private readonly IOptions<SecuriTyOptions> _options;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(IOptions<SecuriTyOptions> options, ILogger<JwtTokenService> logger)
        {
            _options = options;
            _logger = logger;
        }
        public async Task<string> GenerateAccessTokenAsync(Guid userId, string userName, string email)
        {
            var jwt = _options.Value.Jwt;
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(JwtRegisteredClaimNames.Email,email),
                new(JwtRegisteredClaimNames.Name, userName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat,new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var expiration = DateTime.UtcNow.AddMinutes(jwt.AccessTokenExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation(
                "Access token gerado para usuário {userId}, expira em {Expiration}",
                userId, expiration);

            return tokenString;
        }

        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {

            try
            {
                var jwt = _options.Value.Jwt;
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Falha na validação do token: {Error}", ex.Message);
                return null;
            }
        }

    }
}