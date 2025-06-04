using System.Diagnostics.CodeAnalysis;
using CoreReserve.Core.SharedKernel;
using CoreReserve.Domain.Entities.UserAggregate;
using CoreReserve.Infrastructure.Data.Context;
using CoreReserve.Infrastructure.Data.Repositories;
using CoreReserve.Infrastructure.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CoreReserve.Infrastructure.Data
{
    /// <summary>
    /// Classe de configuração dos serviços de infraestrutura da aplicação.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ConfigureServices
    {
        /// <summary>
        /// Adiciona o serviço de cache em memória à coleção de serviços.
        /// </summary>
        /// <param name="services">A coleção de serviços.</param>
        public static void AddMemoryCacheService(this IServiceCollection services) =>
            services.AddScoped<ICacheService, MemoryCacheService>();

        /// <summary>
        /// Adiciona o serviço de cache distribuído à coleção de serviços.
        /// </summary>
        /// <param name="services">A coleção de serviços.</param>
        public static void AddDistributedCacheService(this IServiceCollection services) =>
            services.AddScoped<ICacheService, DistributedCacheService>();

        /// <summary>
        /// Adiciona os serviços essenciais da infraestrutura à coleção de serviços.
        /// </summary>
        /// <param name="services">A coleção de serviços.</param>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services) =>
            services
                .AddScoped<WriteDbContext>()
                .AddScoped<EventStoreDbContext>()
                .AddScoped<IUnitOfWork, UnitOfWork>()
                .AddScoped<IHashService, HashService>();


        /// <summary>
        /// Adiciona os repositórios que oferecem operações apenas de escrita à coleção de serviços.
        /// </summary>
        /// <param name="services">A coleção de serviços.</param>
        public static IServiceCollection AddWriteOnlyRepositories(this IServiceCollection services) =>
             services
                .AddScoped<IEventStoreRepository, EventStoreRepository>()
                .AddScoped<IUserWriteOnlyRepository, UserWriteOnlyRepository>();
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe ConfigureServices → Responsável por registrar serviços fundamentais da infraestrutura. 
    ✅ Métodos AddMemoryCacheService() e AddDistributedCacheService() → Permitem alternar entre cache em memória e cache distribuído. 
    ✅ Método AddInfrastructure() → Registra serviços essenciais como contextos de banco de dados e Unit of Work. 
    ✅ Método AddWriteOnlyRepositories() → Adiciona repositórios que só permitem operações de escrita. 
    ✅ Uso de AddScoped → Define o tempo de vida dos serviços como escopo de requisição, garantindo reutilização eficiente de instâncias. 
    ✅ Essa configuração modulariza a infraestrutura, facilitando testes e manutenção da aplicação. 
    */
}