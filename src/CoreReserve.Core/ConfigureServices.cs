using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using CoreReserve.Core.AppSettings;
using CoreReserve.Core.SharedKernel;


namespace CoreReserve.Core
{
    [ExcludeFromCodeCoverage]
    public static class ConfigureServices
    {
        public static IServiceCollection ConfigureAppSettings(this IServiceCollection services) =>
        services
            .AddOptionsWithValidation<ConnectionOptions>()
            .AddOptionsWithValidation<CacheOptions>();


        private static IServiceCollection AddOptionsWithValidation<TOptions>(this IServiceCollection services)
    where TOptions : class, IAppOptions
        {
            return services
                .AddOptions<TOptions>()
                .BindConfiguration(TOptions.ConfigSectionPath, binderOptions => binderOptions.BindNonPublicProperties = true)
                .ValidateDataAnnotations()
                .ValidateOnStart()
                .Services;
        }
    }
}