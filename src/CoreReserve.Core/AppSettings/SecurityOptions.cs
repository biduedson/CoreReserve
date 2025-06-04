using System.ComponentModel.DataAnnotations;
using CoreReserve.Core.SharedKernel;

namespace CoreReserve.Core.AppSettings
{
    public sealed class SecuriTyOptions : IAppOptions
    {
        static string IAppOptions.ConfigSectionPath => "Security:Brcypt";

        [Required]
        public int WorkFactor { get; private init; }
    }
}