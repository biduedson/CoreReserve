using System.Security.Claims;

namespace CoreReserve.Core.SharedKernel
{
    public interface IJwtTokenService
    {
        Task<string> GenerateAccessTokenAsync(Guid userId, string userName, string email);
        //  Task<string> GenerateRefreshTokenAsync();
        Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
        //Task<bool> ValidateRefreshTokenAsync(string refreshToken, Guid userId);
        // Task RevokeRefreshTokenAsync(string refreshToken);
        // Task RevokeAllUserTokensAsync(Guid userId);
    }
}