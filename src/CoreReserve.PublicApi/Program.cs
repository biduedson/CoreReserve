using System.IO.Compression;
using Asp.Versioning;
using CoreReserve.Core.Extensions;
using CoreReserve.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using CoreReserve.Infrastructure.Data;
using CoreReserve.Application;
using CoreReserve.Query;
using CoreReserve.PublicApi.Extensions;
using CorrelationId.DependencyInjection;
using StackExchange.Profiling;
using FluentValidation;
using FluentValidation.Resources;
using System.Globalization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Scalar.AspNetCore;
using CorrelationId;

/// <summary>
/// Ponto de entrada principal da aplicação Core Reserve API
/// Configura todos os serviços, middlewares e pipeline de requisições HTTP
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Configuração de serviços fundamentais do ASP.NET Core
/// Inclui compressão de resposta, serialização JSON e roteamento
/// </summary>
builder.Services
    .Configure<GzipCompressionProviderOptions>(compressionOptions => compressionOptions.Level = CompressionLevel.Fastest)
    .Configure<JsonOptions>(jsonOptions => jsonOptions.JsonSerializerOptions.Configure())
    .Configure<RouteOptions>(routeOptions => routeOptions.LowercaseUrls = true)
    .AddHttpClient()
    .AddHttpContextAccessor()
    .AddResponseCompression(compressionOptions =>
    {
        compressionOptions.EnableForHttps = true;
        compressionOptions.Providers.Add<GzipCompressionProvider>();
    })
    .AddEndpointsApiExplorer()
    /// <summary>
    /// Configuração de versionamento da API
    /// Define versão padrão e habilita relatórios de versão
    /// </summary>
    .AddApiVersioning(versioningOptions =>
    {
        versioningOptions.DefaultApiVersion = ApiVersion.Default;
        versioningOptions.ReportApiVersions = true;
        versioningOptions.AssumeDefaultVersionWhenUnspecified = true;
    })
    /// <summary>
    /// Configuração do explorador de API para documentação
    /// Define formato de nome de grupo e substituição de versão em URLs
    /// </summary>
    .AddApiExplorer(explorerOptions =>
    {
        explorerOptions.GroupNameFormat = "'v'VVV";
        explorerOptions.SubstituteApiVersionInUrl = true;
    });

/// <summary>
/// Configuração de serviços de documentação e segurança
/// Adiciona OpenAPI, proteção de dados e controladores MVC
/// </summary>
builder.Services.AddOpenApi();
builder.Services.AddDataProtection();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(behaviorOptions =>
    {
        behaviorOptions.SuppressMapClientErrors = true;
        behaviorOptions.SuppressModelStateInvalidFilter = true;
    })
    .AddJsonOptions(_ => { });
builder.Services.AddCors(options =>
{
    options.AddPolicy("ScalarPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Em desenvolvimento: permite qualquer origem
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Em produção: URLs específicas
            policy.WithOrigins("https://your-production-domain.com")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});
/// <summary>
/// Registra todos os serviços específicos da aplicação Core Reserve
/// Inclui configurações, infraestrutura, handlers, repositórios e cache
/// </summary>
builder.Services
    .ConfigureAppSettings()
    .AddInfrastructure()
    .AddCommandHandlers()
    .AddQueryHandlers()
    .AddWriteDbContext(builder.Environment)
    .AddWriteOnlyRepositories()
    .AddReadDbContext()
    .AddReadOnlyRepositories()
    .AddCacheService(builder.Configuration)
    .AddHealthChecks(builder.Configuration)
    .AddDefaultCorrelationId();

/// <summary>
/// Configuração do MiniProfiler para monitoramento de desempenho
/// Permite análise detalhada de performance da aplicação
/// Referência: https://miniprofiler.com/dotnet/
/// </summary>
builder.Services.AddMiniProfiler(options =>
{
    // Rota para exibição do perfil de execução: /profiler/results-index
    options.RouteBasePath = "/profiler";
    options.ColorScheme = ColorScheme.Dark;
    options.EnableServerTimingHeader = true;
    options.TrackConnectionOpenClose = true;
    options.EnableDebugMode = builder.Environment.IsDevelopment();
}).AddEntityFramework();

/// <summary>
/// Validação da configuração do container de dependências
/// Garante que todos os serviços estão corretamente registrados
/// </summary>
builder.Host.UseDefaultServiceProvider((context, serviceProviderOptions) =>
{
    serviceProviderOptions.ValidateScopes = context.HostingEnvironment.IsDevelopment();
    serviceProviderOptions.ValidateOnBuild = true;
});

/// <summary>
/// Configuração específica do servidor Kestrel
/// Remove header de servidor por questões de segurança
/// </summary>
builder.WebHost.UseKestrel(kestrelOptions => kestrelOptions.AddServerHeader = false);

/// <summary>
/// Configuração global do FluentValidation
/// Define resolução de nomes e idioma para validações
/// </summary>
ValidatorOptions.Global.DisplayNameResolver = (_, member, _) => member?.Name;
ValidatorOptions.Global.LanguageManager = new LanguageManager { Enabled = true, Culture = new CultureInfo("en-US") };

var app = builder.Build();

/// <summary>
/// Configuração do pipeline de middlewares para ambiente de desenvolvimento
/// Habilita página de exceções detalhadas para debugging
/// </summary>
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

/// <summary>
/// Configuração de Health Checks para monitoramento de saúde da aplicação
/// Endpoint /health retorna status de todos os serviços dependentes
/// </summary>
app.UseHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

/// <summary>
/// Mapeamento do endpoint OpenAPI para especificação da API
/// Gera documentação automática baseada nos controladores
/// </summary>
app.MapOpenApi();

/// <summary>
/// Configuração do Scalar para interface de documentação da API
/// Substitui o Swagger UI com interface mais moderna
/// Acessível em /scalar/v1
/// </summary>
app.MapScalarApiReference(scalarOptions =>
{
    scalarOptions.DarkMode = true;
    scalarOptions.DotNetFlag = false;
    scalarOptions.HideDownloadButton = true;
    scalarOptions.HideModels = true;
    scalarOptions.Title = "Core Reserve";
});

/// <summary>
/// Configuração do pipeline de middlewares na ordem correta
/// Cada middleware processa requisições na ordem definida
/// </summary>
app.UseCors("ScalarPolicy");       // Politica de cors
app.UseErrorHandling();           // Tratamento global de erros
app.UseResponseCompression();     // Compressão de respostas HTTP
app.UseHttpsRedirection();        // Redirecionamento para HTTPS
app.UseMiniProfiler();           // Profiling de performance
app.UseCorrelationId();          // Rastreamento de requisições
app.UseAuthentication();         // Autenticação de usuários
app.UseAuthorization();          // Autorização de acesso
app.MapControllers();            // Mapeamento de rotas dos controladores

/// <summary>
/// Inicia a aplicação de forma assíncrona
/// </summary>
await app.RunAppAsync();

/*
=============================================================================
EXPLICAÇÃO DETALHADA DO CÓDIGO CORE RESERVE API
=============================================================================

PROPÓSITO GERAL:
Este arquivo Program.cs é o ponto de entrada de uma aplicação ASP.NET Core 
Web API chamada "Core Reserve". Ele configura todos os serviços necessários, 
middlewares e o pipeline de processamento de requisições HTTP.

ARQUITETURA IMPLEMENTADA:
- Clean Architecture com separação de responsabilidades
- CQRS (Command Query Responsibility Segregation) - handlers separados
- Repository Pattern para acesso a dados
- Dependency Injection nativo do ASP.NET Core

SEÇÕES PRINCIPAIS:

1. CONFIGURAÇÃO DE SERVIÇOS (builder.Services):
   - Compressão GZIP para otimizar transferência de dados
   - Serialização JSON personalizada
   - Versionamento de API com suporte a múltiplas versões
   - OpenAPI/Swagger para documentação automática
   - Proteção de dados para criptografia
   - Configuração de controladores MVC

2. SERVIÇOS ESPECÍFICOS DA APLICAÇÃO:
   - ConfigureAppSettings(): Configurações da aplicação
   - AddInfrastructure(): Serviços de infraestrutura
   - AddCommandHandlers()/AddQueryHandlers(): Implementação CQRS
   - AddWriteDbContext()/AddReadDbContext(): Contextos separados para escrita/leitura
   - AddRepositories(): Padrão Repository para acesso a dados
   - AddCacheService(): Sistema de cache distribuído
   - AddHealthChecks(): Monitoramento de saúde dos serviços
   - AddDefaultCorrelationId(): Rastreamento de requisições

3. MONITORAMENTO E DEBUGGING:
   - MiniProfiler: Análise de performance em tempo real
   - Health Checks: Monitoramento de dependências externas
   - Correlation ID: Rastreamento de requisições distribuídas

4. CONFIGURAÇÕES DE SEGURANÇA:
   - Remoção do header Server (reduz surface de ataque)
   - Redirecionamento forçado para HTTPS
   - Proteção de dados integrada
   - Validação de escopos em desenvolvimento

5. PIPELINE DE MIDDLEWARES (ordem importante):
   a) ErrorHandling: Captura e trata exceções globalmente
   b) ResponseCompression: Comprime respostas para economizar banda
   c) HttpsRedirection: Força uso de HTTPS
   d) MiniProfiler: Coleta métricas de performance
   e) CorrelationId: Adiciona ID único para rastreamento
   f) Authentication: Verifica identidade do usuário
   g) Authorization: Verifica permissões de acesso
   h) MapControllers: Roteia requisições para controladores

6. FUNCIONALIDADES ESPECIAIS:
   - Scalar API Reference: Interface moderna para documentação da API
   - FluentValidation: Sistema avançado de validação com i18n
   - Entity Framework profiling: Monitoramento de queries SQL
   - Ambiente condicional: Comportamento diferente por ambiente

PADRÕES DE DESIGN UTILIZADOS:
- Dependency Injection: Inversão de controle
- Builder Pattern: Configuração fluente
- Factory Pattern: Criação de contextos de banco
- Repository Pattern: Abstração de acesso a dados
- CQRS: Separação de comandos e consultas
- Middleware Pattern: Pipeline de processamento

BENEFÍCIOS DA ARQUITETURA:
- Escalabilidade: Separação de contextos de leitura/escrita
- Manutenibilidade: Responsabilidades bem definidas
- Testabilidade: Injeção de dependência facilita testes
- Monitoramento: Observabilidade completa da aplicação
- Performance: Cache, compressão e profiling integrados
- Segurança: Múltiplas camadas de proteção
- Documentação: Geração automática de documentação da API

Esta configuração representa uma aplicação enterprise-ready, com todas as
práticas recomendadas para aplicações .NET modernas em produção.
*/