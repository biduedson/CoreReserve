using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using CoreReserve.Infrastructure.Data.Context;
using CoreReserve.Query.Abstractions;
using CoreReserve.Query.Data.Context;
using NSubstitute;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using Xunit;

namespace CoreReserve.IntegrationTests.Abstractions
{
    /// <summary>
    /// Classe base para testes de integração que configura ambiente de teste isolado
    /// com bancos de dados em memória e factory para aplicação web
    /// </summary>
    public abstract class TestBase : IAsyncLifetime
    {
        #region Constants and Fields

        /// <summary>String de conexão para bancos SQLite em memória</summary>
        private const string ConnectionString = "Data Source=:memory:";

        /// <summary>Conexão SQLite para contexto de Event Store</summary>
        private readonly SqliteConnection _eventStoreDbContextSqlite = new(ConnectionString);

        /// <summary>Conexão SQLite para contexto de escrita principal</summary>
        private readonly SqliteConnection _writeDbContextSqlite = new(ConnectionString);

        #endregion

        #region IAsyncLifetime Implementation

        /// <summary>
        /// Inicializa recursos assíncronos - abre conexões com bancos de dados
        /// </summary>
        public async Task InitializeAsync()
        {
            // Abre conexão com banco de escrita
            await _writeDbContextSqlite.OpenAsync();

            // Abre conexão com banco de event store
            await _eventStoreDbContextSqlite.OpenAsync();
        }

        /// <summary>
        /// Libera recursos assíncronos - fecha e descarta conexões com bancos
        /// </summary>
        public async Task DisposeAsync()
        {
            // Descarta conexão com banco de escrita
            await _writeDbContextSqlite.DisposeAsync();

            // Descarta conexão com banco de event store
            await _eventStoreDbContextSqlite.DisposeAsync();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Configura factory da aplicação web com configurações específicas para testes
        /// </summary>
        /// <param name="configureServices">Ação opcional para configurar serviços adicionais</param>
        /// <param name="configureServiceScope">Ação opcional para configurar escopo de serviços</param>
        /// <returns>Factory configurada para testes</returns>
        internal WebApplicationFactory<Program> InitializeWebAppFactory(
            Action<IServiceCollection> configureServices = null,
            Action<IServiceScope> configureServiceScope = null)
        {
            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(hostBuilder =>
                {
                    #region Environment Configuration

                    // Define ambiente como "Testing" para configurações específicas
                    hostBuilder.UseEnvironment("Testing");

                    #endregion

                    #region Connection Strings Configuration

                    // Configura todas as connection strings para usar bancos em memória
                    hostBuilder.UseSetting("ConnectionStrings:SqlConnection", "InMemory");
                    hostBuilder.UseSetting("ConnectionStrings:NoSqlConnection", "InMemory");
                    hostBuilder.UseSetting("ConnectionStrings:CacheConnection", "InMemory");

                    #endregion

                    #region Cache Configuration

                    // Configura opções de cache com tempos reduzidos para testes
                    hostBuilder.UseSetting("CacheOptions:AbsoluteExpirationInHours", "1");
                    hostBuilder.UseSetting("CacheOptions:SlidingExpirationInSeconds", "30");

                    #endregion

                    #region Security Configuration

                    // Configura parâmetros de segurança para ambiente de teste
                    hostBuilder.UseSetting("Security:Bcrypt:WorkFactor", "12");
                    hostBuilder.UseSetting("Security:Jwt:Audience", "TestAudience");
                    hostBuilder.UseSetting("Security:Jwt:Issuer", "TestIssuer");
                    hostBuilder.UseSetting("Security:Jwt:SecretKey", "test-key");
                    hostBuilder.UseSetting("Security:Jwt:ExpireMinutes", "60");

                    #endregion

                    #region Logging Configuration

                    // Remove todos os providers de log para evitar ruído durante testes
                    hostBuilder.ConfigureLogging(logging => logging.ClearProviders());

                    #endregion

                    #region Services Configuration

                    hostBuilder.ConfigureServices(services =>
                    {
                        #region Service Removal

                        // Remove serviços existentes relacionados a banco de dados
                        services.RemoveAll<DbConnection>();
                        services.RemoveAll<DbContextOptions>();
                        services.RemoveAll<WriteDbContext>();
                        services.RemoveAll<DbContextOptions<WriteDbContext>>();
                        services.RemoveAll<EventStoreDbContext>();
                        services.RemoveAll<DbContextOptions<EventStoreDbContext>>();
                        services.RemoveAll<NoSqlDbContext>();
                        services.RemoveAll<ISynchronizeDb>();

                        #endregion

                        #region Database Context Registration

                        // Registra contexto de escrita com SQLite em memória
                        services.AddDbContext<WriteDbContext>(options =>
                            options.UseSqlite(_writeDbContextSqlite)
                                   .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

                        // Registra contexto de event store com SQLite em memória
                        services.AddDbContext<EventStoreDbContext>(options =>
                            options.UseSqlite(_eventStoreDbContextSqlite)
                                   .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

                        #endregion

                        #region Mock Services Registration

                        // Registra mocks para serviços de leitura e sincronização
                        services.AddSingleton(_ => Substitute.For<IReadDbContext>());
                        services.AddSingleton(_ => Substitute.For<ISynchronizeDb>());

                        #endregion

                        #region Custom Services Configuration

                        // Executa configuração customizada de serviços se fornecida
                        configureServices?.Invoke(services);

                        #endregion

                        #region Database Initialization

                        // Cria provider temporário para inicializar bancos de dados
                        using var provider = services.BuildServiceProvider(true);
                        using var scope = provider.CreateScope();

                        // Obtém contextos de banco de dados
                        var writeDbContext = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
                        var eventStoreDbContext = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();

                        // Cria estrutura dos bancos de dados
                        writeDbContext.Database.EnsureCreated();
                        eventStoreDbContext.Database.EnsureCreated();

                        // Executa configuração customizada de escopo se fornecida
                        configureServiceScope?.Invoke(scope);

                        // Descarta contextos após inicialização
                        writeDbContext.Dispose();
                        eventStoreDbContext.Dispose();

                        #endregion
                    });

                    #endregion
                });
        }
        /// <summary>
        /// Cria opções padrão para o cliente HTTP dos testes
        /// AllowAutoRedirect = false impede redirecionamentos automáticos para testar responses específicas
        /// </summary>
        protected internal static WebApplicationFactoryClientOptions CreateClientOptions() => new() { AllowAutoRedirect = false };
        #endregion
    }
}
#region EXPLICAÇÃO GERAL DA CLASSE TestBase
/*
 * EXPLICAÇÃO GERAL DA CLASSE TestBase:
 * 
 * Esta classe serve como base para testes de integração no projeto CoreReserve,
 * implementando o padrão de configuração de ambiente isolado para testes.
 * 
 * PRINCIPAIS FUNCIONALIDADES:
 * 
 * 1. ISOLAMENTO DE DADOS:
 *    - Utiliza bancos SQLite em memória (:memory:) para garantir que cada teste
 *      execute com dados limpos e não interfira em outros testes
 *    - Dois contextos separados: WriteDbContext (escrita) e EventStoreDbContext (eventos)
 * 
 * 2. CONFIGURAÇÃO DE AMBIENTE:
 *    - Define ambiente como "Testing" para ativar configurações específicas
 *    - Sobrescreve connection strings para usar bancos em memória
 *    - Configura parâmetros de cache, segurança e JWT para testes
 * 
 * 3. SUBSTITUIÇÃO DE DEPENDÊNCIAS:
 *    - Remove serviços reais de banco de dados e os substitui por versões de teste
 *    - Utiliza mocks (NSubstitute) para serviços de leitura e sincronização
 *    - Permite injeção de configurações customizadas via parâmetros opcionais
 * 
 * 4. GERENCIAMENTO DE CICLO DE VIDA:
 *    - Implementa IAsyncLifetime para controle preciso de inicialização/limpeza
 *    - Abre conexões no InitializeAsync() e as fecha no DisposeAsync()
 *    - Garante que recursos sejam liberados adequadamente após cada teste
 * 
 * 5. CLIENTE HTTP CONFIGURADO:
 *    - Fornece HttpClient pré-configurado para testes de API
 *    - Desabilita redirecionamentos automáticos para controle total nas assertions
 * 
 * VANTAGENS DESTA ABORDAGEM:
 * - Testes rápidos (bancos em memória)
 * - Isolamento completo entre testes
 * - Configuração centralizada e reutilizável
 * - Flexibilidade para customizações específicas
 * - Controle total sobre dependências e configurações
 */
#endregion