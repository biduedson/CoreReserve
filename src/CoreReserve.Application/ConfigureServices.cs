using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CoreReserve.Application.Abstractions;
using CoreReserve.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CoreReserve.Application
{
    /// <summary>
    /// Classe de configuração de serviços para a camada de aplicação.
    /// Responsável por registrar handlers de comandos e comportamentos no MediatR.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ConfigureServices
    {
        /// <summary>
        /// Adiciona handlers de comando à coleção de serviços.
        /// </summary>
        /// <param name="services">A coleção de serviços.</param>
        /// <returns>A coleção de serviços configurada.</returns>
        public static IServiceCollection AddCommandHandlers(this IServiceCollection services)
        {
            var assembly = Assembly.GetAssembly(typeof(IApplicationMarker));
            return services
                .AddValidatorsFromAssembly(assembly, ServiceLifetime.Singleton)
                .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly)
                    .AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>)));
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe ConfigureServices → Responsável por registrar serviços da aplicação na injeção de dependência. 
    ✅ Método AddCommandHandlers() → Registra handlers de comando e validadores na coleção de serviços. 
    ✅ Uso de FluentValidation → Permite validação de comandos com regras definidas em classes separadas. 
    ✅ Uso de MediatR → Garante comunicação entre componentes desacoplados através de comandos e eventos. 
    ✅ Adição do LoggingBehavior → Insere logging no pipeline do MediatR para rastrear execução de comandos. 
    ✅ Essa abordagem melhora a modularidade, escalabilidade e testabilidade da aplicação. 
    */

}