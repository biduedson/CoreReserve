using System.ComponentModel.DataAnnotations;
using CoreReserve.Core.SharedKernel;

namespace CoreReserve.Core.AppSettings
{
    public sealed class SecuriTyOptions : IAppOptions
    {
        static string IAppOptions.ConfigSectionPath => "Security";

        [Required]
        public BcryptOptions Bcrypt { get; private init; }

        [Required]
        public JwtOptions Jwt { get; private init; }
    }
}