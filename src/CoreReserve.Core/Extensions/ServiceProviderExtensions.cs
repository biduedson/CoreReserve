
using Microsoft.Extensions.DependencyInjection;
using CoreReserve.Core.SharedKernel;
using Microsoft.Extensions.Options;

namespace CoreReserve.Core.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static TOptions GetOptions<TOptions>(this IServiceProvider serviceProvider)
    where TOptions : class, IAppOptions =>
    serviceProvider.GetService<IOptions<TOptions>>()?.Value;
    }
}