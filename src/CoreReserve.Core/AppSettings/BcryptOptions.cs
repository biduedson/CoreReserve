using System.ComponentModel.DataAnnotations;
using CoreReserve.Core.SharedKernel;

namespace CoreReserve.Core.AppSettings
{
    public sealed class BcryptOptions : IAppOptions
    {
        static string IAppOptions.ConfigSectionPath => "Security:Bcrypt";

        [Required]
        public string WorkFactor { get; private init; }
    }
}