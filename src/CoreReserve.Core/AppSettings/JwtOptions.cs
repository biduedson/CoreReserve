using System.ComponentModel.DataAnnotations;
using CoreReserve.Core.SharedKernel;

namespace CoreReserve.Core.AppSettings
{
    public sealed class JwtOptions : IAppOptions
    {
        public static string ConfigSectionPath => "Security:Jwt";

        [Required]
        public string Issuer { get; private init; }

        [Required]
        public string Audience { get; private init; }

        [Required]
        public int AccessTokenExpirationMinutes { get; private init; }

        [Required]
        public int RefreshTokenExpirationDays { get; private init; }

        [Required]
        public string SecretKey { get; private init; }
    }
}