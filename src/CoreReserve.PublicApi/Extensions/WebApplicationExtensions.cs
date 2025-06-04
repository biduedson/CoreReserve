using System;
using System.Threading.Tasks;
using AutoMapper;
using CoreReserve.Infrastructure.Data.Context;
using CoreReserve.Query.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace CoreReserve.PublicApi.Extensions
{
    /// <summary>
    /// Classe de extensão para a configuração do ciclo de vida da aplicação Web.
    /// Responsável por validação de mapeamentos, migração de banco de dados e inicialização da API.
    /// </summary>
    internal static class WebApplicationExtensions
    {
        /// <summary>
        /// Executa o ciclo de inicialização completo da aplicação, incluindo configuração do AutoMapper e migração de bancos de dados.
        /// </summary>
        /// <param name="app">Instância da aplicação Web.</param>
        public static async Task RunAppAsync(this WebApplication app)
        {
            await using var serviceScope = app.Services.CreateAsyncScope();

            var mapper = serviceScope.ServiceProvider.GetRequiredService<IMapper>();

            app.Logger.LogInformation("----- AutoMapper: validando os mapeamentos...");

            // Valida se as configurações do AutoMapper estão corretas antes da inicialização.
            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            // Compila os mapeamentos para melhorar a performance das conversões entre objetos.
            mapper.ConfigurationProvider.CompileMappings();

            app.Logger.LogInformation("----- AutoMapper: mapeamentos validados com sucesso!");

            // Executa a migração dos bancos de dados para garantir que estão atualizados.
            await app.MigrateDataBasesAsync(serviceScope);

            app.Logger.LogInformation("----- Aplicação está iniciando...");

            await app.RunAsync();
        }

        /// <summary>
        /// Realiza a migração dos bancos de dados da aplicação, garantindo que estejam atualizados.
        /// </summary>
        /// <param name="app">Instância da aplicação Web.</param>
        /// <param name="serviceScope">Escopo do serviço para obter dependências.</param>
        private static async Task MigrateDataBasesAsync(this WebApplication app, AsyncServiceScope serviceScope)
        {
            await using var writeDbContext = serviceScope.ServiceProvider.GetRequiredService<WriteDbContext>();
            await using var eventStoreDbContext = serviceScope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
            using var readDbContext = serviceScope.ServiceProvider.GetRequiredService<IReadDbContext>();

            try
            {
                await app.MigrateDbContextAsync(writeDbContext);
                await app.MigrateDbContextAsync(eventStoreDbContext);
                await app.MigrateMongoDbContextAsync(readDbContext);
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "Ocorreu uma exceção ao inicializar a aplicação: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Realiza a migração do banco de dados relacional utilizando Entity Framework.
        /// </summary>
        /// <typeparam name="TDbContext">Tipo do contexto do banco de dados.</typeparam>
        /// <param name="app">Instância da aplicação Web.</param>
        /// <param name="dbContext">Contexto do banco de dados a ser migrado.</param>
        private static async Task MigrateDbContextAsync<TDbContext>(this WebApplication app, TDbContext dbContext)
            where TDbContext : DbContext
        {
            var dbName = dbContext.Database.GetDbConnection().Database;

            app.Logger.LogInformation("----- {DbName}: verificando migrações pendentes...", dbName);

            if (dbContext.Database.HasPendingModelChanges())
            {
                app.Logger.LogInformation("----- {DbName}: criando e migrando o banco de dados...", dbName);

                await dbContext.Database.MigrateAsync();

                app.Logger.LogInformation("----- {DbName}: banco de dados migrado com sucesso!", dbName);
            }
            else
            {
                app.Logger.LogInformation("----- {DbName}: todas as migrações estão atualizadas.", dbName);
            }
        }

        /// <summary>
        /// Executa a criação de coleções no banco de dados MongoDB para garantir que as estruturas estejam disponíveis.
        /// </summary>
        /// <param name="app">Instância da aplicação Web.</param>
        /// <param name="readDbContext">Contexto de leitura do MongoDB.</param>
        private static async Task MigrateMongoDbContextAsync(this WebApplication app, IReadDbContext readDbContext)
        {
            app.Logger.LogInformation("----- MongoDB: criando coleções...");

            await readDbContext.CreateCollectionsAsync();

            app.Logger.LogInformation("----- MongoDB: coleções criadas com sucesso!");
        }
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe WebApplicationExtensions → Gerencia o ciclo de inicialização e migração dos bancos de dados.
✅ Método RunAppAsync() → Executa validações, migrações e inicializa a aplicação Web.
✅ Método MigrateDataBasesAsync() → Organiza a migração de bancos relacionais (Entity Framework) e NoSQL (MongoDB).
✅ Método MigrateDbContextAsync() → Atualiza bancos de dados relacionais, verificando migrações pendentes.
✅ Método MigrateMongoDbContextAsync() → Garante a criação de coleções no banco MongoDB antes da execução da aplicação.
✅ Uso de AutoMapper → Valida e compila mapeamentos para melhor performance na conversão de objetos.
✅ Uso de logging → Registra eventos importantes para auditoria e diagnóstico de erros.
✅ Arquitetura baseada em extensões → Modulariza o processo de inicialização, tornando o código mais organizado e reutilizável.
✅ Essa abordagem melhora a confiabilidade da aplicação e facilita a manutenção do banco de dados.
*/
