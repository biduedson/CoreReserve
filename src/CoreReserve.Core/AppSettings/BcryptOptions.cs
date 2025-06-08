using System.ComponentModel.DataAnnotations;
using CoreReserve.Core.SharedKernel;

namespace CoreReserve.Core.AppSettings
{
    public sealed class BcryptOptions : IAppOptions
    {
        public static string ConfigSectionPath => "Security:Bcrypt";

        [Required]
        public int WorkFactor { get; private init; }
    }
}