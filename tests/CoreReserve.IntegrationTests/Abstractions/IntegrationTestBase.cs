using System.Data.Common;
using CoreReserve.Infrastructure.Data.Context;
using CoreReserve.Query.Abstractions;
using CoreReserve.Query.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CoreReserve.IntegrationTests.Abstractions
{
    /// <summary>
    /// Classe base abstrata para testes de integração que configura o ambiente de teste
    /// com banco de dados em memória (SQLite) e factory de aplicação web.
    /// Implementa IAsyncLifetime para gerenciar o ciclo de vida das conexões de banco.
    /// Esta classe fornece toda a infraestrutura necessária para executar testes de integração
    /// simulando um ambiente completo da aplicação com bancos de dados isolados.
    /// </summary>
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        #region Constants - Configurações de Conexão

        /// <summary>
        /// String de conexão padronizada para bancos SQLite em memória.
        /// Utilizada para criar bancos temporários durante a execução dos testes,
        /// garantindo isolamento entre diferentes execuções de teste.
        /// </summary>
        protected const string ConnectionString = "Data Source=:memory:";

        #endregion

        #region Private Fields - Recursos de Banco de Dados

        /// <summary>
        /// Conexão SQLite em memória dedicada ao Event Store (eventos de domínio).
        /// Armazena todos os eventos gerados durante as operações de domínio,
        /// permitindo rastreamento e auditoria das mudanças de estado.
        /// Mantida aberta durante todo o ciclo de vida do teste para preservar dados.
        /// </summary>
        private readonly SqliteConnection _eventStoreDbContextSqlite = new(ConnectionString);

        /// <summary>
        /// Conexão SQLite em memória dedicada às operações de escrita (comandos).
        /// Representa o banco principal onde são executadas as operações de CRUD
        /// da aplicação durante os testes de integração.
        /// Mantida aberta durante todo o ciclo de vida do teste para preservar dados.
        /// </summary>
        private readonly SqliteConnection _writeDbContextSqlite = new(ConnectionString);

        #endregion

        #region IAsyncLifetime Implementation - Gerenciamento do Ciclo de Vida

        /// <summary>
        /// Inicialização assíncrona executada antes de cada teste.
        /// Abre as conexões com os bancos de dados em memória, preparando
        /// o ambiente isolado para execução do teste.
        /// Este método é chamado automaticamente pelo framework xUnit.
        /// </summary>
        /// <returns>Task representando a operação assíncrona de inicialização</returns>
        public virtual async Task InitializeAsync()
        {
            await _writeDbContextSqlite.OpenAsync();
            await _eventStoreDbContextSqlite.OpenAsync();
        }

        /// <summary>
        /// Limpeza assíncrona executada após cada teste.
        /// Fecha e libera todas as conexões de banco de dados para evitar
        /// vazamentos de memória e garantir que cada teste inicie com ambiente limpo.
        /// Este método é chamado automaticamente pelo framework xUnit.
        /// </summary>
        /// <returns>Task representando a operação assíncrona de limpeza</returns>
        public virtual async Task DisposeAsync()
        {
            await _writeDbContextSqlite.DisposeAsync();
            await _eventStoreDbContextSqlite.DisposeAsync();
        }

        #endregion

        #region Public Methods - Interface para Testes

        /// <summary>
        /// Cria e configura um cliente HTTP padrão para testes simples.
        /// Usa configuração básica sem dados pré-populados no banco.
        /// Ideal para testes de criação de novos recursos.
        /// </summary>
        /// <returns>HttpClient configurado para testes básicos</returns>
        public HttpClient CreateClient() =>
            InitializeWebAppFactory().CreateClient(CreateClientOptions());

        /// <summary>
        /// Cria cliente HTTP com configuração personalizada de serviços.
        /// Permite injeção de mocks específicos ou configurações especiais.
        /// </summary>
        /// <param name="configureServices">Ação para configurar serviços específicos do teste</param>
        /// <returns>HttpClient com configurações personalizadas</returns>
        public HttpClient CreateClientWithServices(Action<IServiceCollection> configureServices) =>
            InitializeWebAppFactory(configureServices: configureServices).CreateClient(CreateClientOptions());

        /// <summary>
        /// Cria cliente HTTP com dados pré-populados no banco de dados.
        /// Ideal para testes que precisam de dados existentes (duplicatas, relacionamentos, etc.).
        /// Executa a configuração de dados DURANTE a criação da factory, garantindo que 
        /// os dados estejam disponíveis quando o cliente fizer requisições.
        /// </summary>
        /// <param name="configureServiceScope">Ação para popular dados no banco antes dos testes</param>
        /// <returns>HttpClient com banco pré-populado</returns>
        public HttpClient CreateClientWithData(Action<IServiceScope> configureServiceScope) =>
            InitializeWebAppFactory(configureServiceScope: configureServiceScope).CreateClient(CreateClientOptions());

        /// <summary>
        /// Cria cliente HTTP com configuração completa personalizada.
        /// Permite tanto configuração de serviços quanto população de dados.
        /// Uso avançado para cenários complexos de teste.
        /// </summary>
        /// <param name="configureServices">Configuração personalizada de serviços</param>
        /// <param name="configureServiceScope">População de dados no banco</param>
        /// <returns>HttpClient totalmente personalizado</returns>
        public HttpClient CreateClientWithFullConfiguration(
            Action<IServiceCollection> configureServices = null!,
            Action<IServiceScope> configureServiceScope = null!) =>
            InitializeWebAppFactory(configureServices, configureServiceScope).CreateClient(CreateClientOptions());

        /// <summary>
        /// Cria opções padronizadas para configuração do cliente HTTP nos testes.
        /// AllowAutoRedirect = false previne redirecionamentos automáticos,
        /// permitindo testar responses específicas como 302, 301, etc.
        /// </summary>
        /// <returns>Opções configuradas para o cliente HTTP de testes</returns>
        protected static WebApplicationFactoryClientOptions CreateClientOptions() =>
            new() { AllowAutoRedirect = false };

        #endregion

        #region Private Methods - Configuração da WebApplicationFactory

        /// <summary>
        /// Inicializa e configura a WebApplicationFactory para simular o ambiente completo da aplicação.
        /// Este método é o ponto central de configuração, onde são substituídas as implementações
        /// reais por mocks e configurados os bancos de dados em memória.
        /// Permite customização adicional através dos parâmetros opcionais.
        /// </summary>
        /// <param name="configureServices">Ação opcional para configurar serviços adicionais específicos do teste</param>
        /// <param name="configureServiceScope">Ação opcional para configurar o escopo de serviços (ex: popular banco com dados de teste)</param>
        /// <returns>WebApplicationFactory totalmente configurada e pronta para uso</returns>
        private WebApplicationFactory<Program> InitializeWebAppFactory(
            Action<IServiceCollection> configureServices = null!,
            Action<IServiceScope> configureServiceScope = null!)
        {
            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(hostBuilder =>
                {
                    ConfigureHostSettings(hostBuilder);
                    ConfigureServices(hostBuilder, configureServices, configureServiceScope);
                });
        }

        #endregion

        #region Private Methods - Configuração do Host

        /// <summary>
        /// Configura todas as configurações básicas do host para ambiente de teste.
        /// Define strings de conexão, parâmetros de cache, configurações de segurança
        /// e outras configurações essenciais para simular o ambiente de produção
        /// de forma controlada e isolada.
        /// </summary>
        /// <param name="hostBuilder">Builder do host web para aplicar as configurações</param>
        private static void ConfigureHostSettings(IWebHostBuilder hostBuilder)
        {
            // Configuração de strings de conexão para usar bancos em memória
            hostBuilder.UseSetting("ConnectionStrings:SqlConnection", "InMemory");
            hostBuilder.UseSetting("ConnectionStrings:NoSqlConnection", "InMemory");
            hostBuilder.UseSetting("ConnectionStrings:CacheConnection", "InMemory");

            // Configuração de opções de cache otimizadas para testes
            hostBuilder.UseSetting("CacheOptions:AbsoluteExpirationInHours", "1");
            hostBuilder.UseSetting("CacheOptions:SlidingExpirationInSeconds", "30");

            // Configurações de segurança simplificadas para ambiente de teste
            hostBuilder.UseSetting("Security:Bcrypt:WorkFactor", "12");
            hostBuilder.UseSetting("Security:Jwt:Audience", "TestAudience");
            hostBuilder.UseSetting("Security:Jwt:Issuer", "TestIssuer");
            hostBuilder.UseSetting("Security:Jwt:SecretKey", "test-key");
            hostBuilder.UseSetting("Security:Jwt:ExpireMinutes", "60");

            // Define ambiente específico para testes
            hostBuilder.UseEnvironment("Testing");

            // Remove logging para manter saída dos testes limpa
            hostBuilder.ConfigureLogging(logging => logging.ClearProviders());
        }

        #endregion

        #region Private Methods - Configuração de Serviços

        /// <summary>
        /// Orquestra a configuração completa dos serviços da aplicação para ambiente de teste.
        /// Remove serviços de produção, substitui por versões de teste, configura bancos
        /// em memória e permite customizações específicas através dos parâmetros.
        /// </summary>
        /// <param name="hostBuilder">Builder do host web</param>
        /// <param name="configureServices">Configurações adicionais de serviços específicas do teste</param>
        /// <param name="configureServiceScope">Configurações do escopo de serviços (popular dados, etc.)</param>
        private void ConfigureServices(
            IWebHostBuilder hostBuilder,
            Action<IServiceCollection> configureServices,
            Action<IServiceScope> configureServiceScope)
        {
            hostBuilder.ConfigureServices(services =>
            {
                RemoveProductionServices(services);
                AddTestServices(services);

                // Executa configurações adicionais de serviços se fornecidas
                configureServices?.Invoke(services);

                SetupTestDatabase(services, configureServiceScope);
            });
        }

        /// <summary>
        /// Remove todos os serviços de produção que precisam ser substituídos por versões de teste.
        /// Isso inclui conexões de banco de dados reais, contextos de Entity Framework
        /// e outros serviços que dependem de recursos externos.
        /// Garante que nenhum serviço de produção seja utilizado durante os testes.
        /// </summary>
        /// <param name="services">Coleção de serviços da aplicação</param>
        private static void RemoveProductionServices(IServiceCollection services)
        {
            // Remove todas as implementações de banco de dados de produção
            services.RemoveAll<DbConnection>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<WriteDbContext>();
            services.RemoveAll<DbContextOptions<WriteDbContext>>();
            services.RemoveAll<EventStoreDbContext>();
            services.RemoveAll<DbContextOptions<EventStoreDbContext>>();
            services.RemoveAll<NoSqlDbContext>();
            services.RemoveAll<ISynchronizeDb>();
        }

        /// <summary>
        /// Adiciona todos os serviços específicos necessários para execução dos testes.
        /// Configura contextos de banco em memória com SQLite, adiciona mocks para
        /// serviços externos e configura warnings apropriados para ambiente de teste.
        /// </summary>
        /// <param name="services">Coleção de serviços da aplicação</param>
        private void AddTestServices(IServiceCollection services)
        {
            // Configuração do contexto de escrita com SQLite em memória
            services.AddDbContext<WriteDbContext>(options => options
                .UseSqlite(_writeDbContextSqlite)
                .ConfigureWarnings(warningBuilder =>
                    warningBuilder.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

            // Configuração do contexto de Event Store com SQLite em memória
            services.AddDbContext<EventStoreDbContext>(options => options
                .UseSqlite(_eventStoreDbContextSqlite)
                .ConfigureWarnings(warningBuilder =>
                    warningBuilder.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

            // Adiciona mocks para serviços que não precisam de implementação real nos testes
            services.AddSingleton(_ => Substitute.For<IReadDbContext>());
            services.AddSingleton(_ => Substitute.For<ISynchronizeDb>());
        }

        #endregion

        #region Private Methods - Configuração de Banco de Dados

        /// <summary>
        /// Configura e inicializa todos os bancos de dados necessários para os testes.
        /// Cria as estruturas de banco (tabelas, índices, etc.), executa configurações
        /// personalizadas como população de dados de teste e garante que tudo esteja
        /// pronto antes da execução dos testes.
        /// </summary>
        /// <param name="services">Coleção de serviços configurados</param>
        /// <param name="configureServiceScope">Ação opcional para configurar dados de teste</param>
        private static void SetupTestDatabase(IServiceCollection services, Action<IServiceScope> configureServiceScope)
        {
            // Cria o provider de serviços temporário para configuração inicial
            using var serviceProvider = services.BuildServiceProvider(true);
            using var serviceScope = serviceProvider.CreateScope();

            // Inicialização do banco de escrita - cria todas as tabelas e estruturas
            var writeDbContext = serviceScope.ServiceProvider.GetRequiredService<WriteDbContext>();
            writeDbContext.Database.EnsureCreated();

            // Inicialização do Event Store - cria estruturas para armazenamento de eventos
            var eventStoreDbContext = serviceScope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
            eventStoreDbContext.Database.EnsureCreated();

            // Executa configurações personalizadas (popular dados de teste, configurar estado inicial, etc.)
            configureServiceScope?.Invoke(serviceScope);

            // Limpeza dos contextos após configuração inicial
            writeDbContext.Dispose();
            eventStoreDbContext.Dispose();
        }

        #endregion
    }
}