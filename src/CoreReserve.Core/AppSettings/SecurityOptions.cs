using System.ComponentModel.DataAnnotations;
using CoreReserve.Core.SharedKernel;

namespace CoreReserve.Core.AppSettings
{
    public sealed class SecuriTyOptions : IAppOptions
    {
        public static string ConfigSectionPath => "Security";

        [Required]
        public BcryptOptions Bcrypt { get; private init; }

        [Required]
        public JwtOptions Jwt { get; private init; }
    }
}