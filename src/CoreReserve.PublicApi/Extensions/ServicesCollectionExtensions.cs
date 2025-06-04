using System;
using System.Diagnostics.CodeAnalysis;
using CoreReserve.Core.AppSettings;
using CoreReserve.Core.Extensions;
using CoreReserve.Infrastructure.Data;
using CoreReserve.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace CoreReserve.PublicApi.Extensions
{
    /// <summary>
    /// Extensões para registro de serviços no contêiner de injeção de dependência.
    /// Responsável pela configuração de banco de dados, cache e monitoramento de saúde.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class ServicesCollectionExtensions
    {
        private const int DbMaxRetryCount = 3;
        private const int DbCommandTimeout = 30;
        private const string DbMigrationAssemblyName = "CoreReserve.PublicApi";
        private const string RedisInstanceName = "master";
        private const string TestingEnvironmentName = "Testing";

        private static readonly string[] DbRelationalTags = ["database", "ef-core", "sql-server", "relational"];
        private static readonly string[] DbNoSqlTags = ["database", "mongodb", "no-sql"];

        /// <summary>
        /// Adiciona verificações de saúde para monitoramento de serviços.
        /// Inclui verificações para banco de dados relacional e NoSQL, além do cache.
        /// </summary>
        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetOptions<ConnectionOptions>();

            var healthCheckBuilder = services
                .AddHealthChecks()
                .AddDbContextCheck<WriteDbContext>(tags: DbRelationalTags)
                .AddDbContextCheck<EventStoreDbContext>(tags: DbRelationalTags)
                .AddMongoDb(clientFactory: _ => new MongoClient(options.NoSqlConnection), tags: DbNoSqlTags);

            if (!options.CacheConnectionInMemory())
                healthCheckBuilder.AddRedis(options.CacheConnection);

            return services;
        }

        /// <summary>
        /// Configura o contexto de banco de dados para escrita e eventos.
        /// </summary>
        public static IServiceCollection AddWriteDbContext(this IServiceCollection services, IWebHostEnvironment environment)
        {
            if (!environment.IsEnvironment(TestingEnvironmentName))
            {
                services.AddDbContextPool<WriteDbContext>((serviceProvider, optionsBuilder) =>
                    ConfigureDbContext<WriteDbContext>(
                        serviceProvider, optionsBuilder, QueryTrackingBehavior.TrackAll));

                services.AddDbContextPool<EventStoreDbContext>((serviceProvider, optionsBuilder) =>
                    ConfigureDbContext<EventStoreDbContext>(
                        serviceProvider, optionsBuilder, QueryTrackingBehavior.NoTrackingWithIdentityResolution));
            }

            return services;
        }

        /// <summary>
        /// Adiciona suporte a cache distribuído ou em memória, dependendo da configuração.
        /// </summary>
        public static IServiceCollection AddCacheService(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetOptions<ConnectionOptions>();
            if (options.CacheConnectionInMemory())
            {
                services.AddMemoryCacheService();
                services.AddMemoryCache(memoryOptions => memoryOptions.TrackStatistics = true);
            }
            else
            {
                services.AddDistributedCacheService();
                services.AddStackExchangeRedisCache(redisOptions =>
                {
                    redisOptions.InstanceName = RedisInstanceName;
                    redisOptions.Configuration = options.CacheConnection;
                });
            }

            return services;
        }

        /// <summary>
        /// Configuração personalizada para contextos de banco de dados.
        /// Define comportamento de rastreamento e estratégias de erro.
        /// </summary>
        private static void ConfigureDbContext<TDbContext>(
            IServiceProvider serviceProvider,
            DbContextOptionsBuilder optionsBuilder,
            QueryTrackingBehavior queryTrackingBehavior) where TDbContext : DbContext
        {
            var logger = serviceProvider.GetRequiredService<ILogger<TDbContext>>();
            var options = serviceProvider.GetOptions<ConnectionOptions>();
            var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            var envIsDevelopment = environment.IsDevelopment();

            optionsBuilder
                .UseSqlServer(options.SqlConnection, sqlServerOptions =>
                {
                    sqlServerOptions
                        .MigrationsAssembly(DbMigrationAssemblyName)
                        .EnableRetryOnFailure(DbMaxRetryCount)
                        .CommandTimeout(DbCommandTimeout);
                })
                .EnableDetailedErrors(envIsDevelopment)
                .EnableSensitiveDataLogging(envIsDevelopment)
                .UseQueryTrackingBehavior(queryTrackingBehavior)
                .LogTo((eventId, _) => eventId.Id == CoreEventId.ExecutionStrategyRetrying, eventData =>
                {
                    if (eventData is not ExecutionStrategyEventData retryEventData)
                        return;

                    var exceptions = retryEventData.ExceptionsEncountered;

                    logger.LogWarning(
                        "----- DbContext: Tentativa #{Count} com atraso de {Delay} devido ao erro: {Message}",
                        exceptions.Count,
                        retryEventData.Delay,
                        exceptions[^1].Message);
                });

            if (envIsDevelopment)
                optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
// -----------------------------------------
/*
✅ Classe ServicesCollectionExtensions → Define extensões para configuração de serviços como banco de dados, cache e monitoramento de saúde. 
✅ Método AddHealthChecks() → Adiciona verificações de saúde para SQL Server, MongoDB e Redis. 
✅ Método AddWriteDbContext() → Configura contextos de banco de dados para escrita e eventos usando pooling. 
✅ Método AddCacheService() → Permite configurar cache em memória ou distribuído com Redis. 
✅ Método ConfigureDbContext() → Define estratégias de erro, rastreamento de queries e políticas de retry para banco de dados. 
✅ Essa estrutura modulariza a configuração de serviços, melhorando a manutenção e escalabilidade da aplicação. 
*/
