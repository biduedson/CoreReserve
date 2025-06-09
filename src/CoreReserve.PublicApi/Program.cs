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
using CoreReserve.Infrastructure.Security.Extensions;

#region Application Configuration

/// <summary>
/// Ponto de entrada principal da aplicação Core Reserve API.
/// Configura todos os serviços, middlewares e pipeline de requisições HTTP
/// seguindo padrões de Clean Architecture e CQRS.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

#endregion

#region Core Services Configuration

/// <summary>
/// Configuração de serviços fundamentais do ASP.NET Core incluindo
/// compressão de resposta, serialização JSON customizada e roteamento otimizado.
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
    .AddEndpointsApiExplorer();

#endregion

#region API Versioning Configuration

/// <summary>
/// Configuração avançada de versionamento da API com suporte a múltiplas versões
/// simultâneas e documentação automática de versões disponíveis.
/// </summary>
builder.Services
    .AddApiVersioning(versioningOptions =>
    {
        versioningOptions.DefaultApiVersion = ApiVersion.Default;
        versioningOptions.ReportApiVersions = true;
        versioningOptions.AssumeDefaultVersionWhenUnspecified = true;
    })
    .AddApiExplorer(explorerOptions =>
    {
        explorerOptions.GroupNameFormat = "'v'VVV";
        explorerOptions.SubstituteApiVersionInUrl = true;
    });

#endregion

#region Documentation and Security Services

/// <summary>
/// Configuração de serviços de documentação OpenAPI, proteção de dados
/// e controladores MVC com comportamento personalizado para APIs.
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

#endregion

#region CORS Configuration

/// <summary>
/// Configuração de política CORS diferenciada por ambiente.
/// Desenvolvimento: permissivo para facilitar testes.
/// Produção: restritivo para segurança.
/// </summary>
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

#endregion

#region Application-Specific Services

/// <summary>
/// Registro de todos os serviços específicos da aplicação Core Reserve
/// seguindo arquitetura Clean Architecture com CQRS, Repository Pattern
/// e separação de contextos de leitura/escrita para otimização de performance.
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
    .AddDefaultCorrelationId()
    .AddJwtAuthentication(builder.Configuration);

#endregion

#region Performance Monitoring

/// <summary>
/// Configuração do MiniProfiler para monitoramento detalhado de performance.
/// Permite análise em tempo real de queries SQL, tempo de resposta
/// e gargalos de performance da aplicação.
/// Documentação: https://miniprofiler.com/dotnet/
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

#endregion

#region Service Provider Validation

/// <summary>
/// Configuração de validação rigorosa do container de dependências.
/// Garante detecção precoce de problemas de configuração de DI
/// e valida escopos de serviços em ambiente de desenvolvimento.
/// </summary>
builder.Host.UseDefaultServiceProvider((context, serviceProviderOptions) =>
{
    serviceProviderOptions.ValidateScopes = context.HostingEnvironment.IsDevelopment();
    serviceProviderOptions.ValidateOnBuild = true;
});

#endregion

#region Kestrel Server Configuration

/// <summary>
/// Configuração específica do servidor Kestrel com remoção do header
/// de servidor por questões de segurança (security through obscurity).
/// </summary>
builder.WebHost.UseKestrel(kestrelOptions => kestrelOptions.AddServerHeader = false);

#endregion

#region FluentValidation Global Settings

/// <summary>
/// Configuração global do FluentValidation para padronização
/// de resolução de nomes de propriedades e idioma das mensagens de validação.
/// </summary>
ValidatorOptions.Global.DisplayNameResolver = (_, member, _) => member?.Name;
ValidatorOptions.Global.LanguageManager = new LanguageManager { Enabled = true, Culture = new CultureInfo("en-US") };

#endregion

#region Application Pipeline

/// <summary>
/// Construção da aplicação com todos os serviços configurados.
/// </summary>
var app = builder.Build();

#endregion

#region Development Environment Configuration

/// <summary>
/// Configuração específica para ambiente de desenvolvimento.
/// Habilita página detalhada de exceções para facilitar debugging
/// e resolução de problemas durante o desenvolvimento.
/// </summary>
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

#endregion

#region Health Checks Endpoint

/// <summary>
/// Configuração do endpoint de Health Checks para monitoramento
/// automático da saúde da aplicação e suas dependências.
/// Endpoint acessível em: GET /health
/// </summary>
app.UseHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

#endregion

#region OpenAPI Documentation

/// <summary>
/// Mapeamento do endpoint OpenAPI para geração automática
/// da especificação da API baseada nos controladores e modelos.
/// </summary>
app.MapOpenApi();

#endregion

#region Scalar API Documentation

/// <summary>
/// Configuração do Scalar como interface moderna de documentação da API.
/// Substitui o Swagger UI tradicional com uma experiência mais rica.
/// Interface acessível em: /scalar/v1
/// </summary>
app.MapScalarApiReference(scalarOptions =>
{
    scalarOptions.DarkMode = true;
    scalarOptions.DotNetFlag = false;
    scalarOptions.HideDownloadButton = true;
    scalarOptions.HideModels = true;
    scalarOptions.Title = "Core Reserve";
});

#endregion

#region Middleware Pipeline

/// <summary>
/// Configuração do pipeline de middlewares na ordem específica.
/// A ordem é crítica pois cada middleware processa requisições
/// sequencialmente e pode afetar o comportamento dos subsequentes.
/// </summary>
app.UseCors("ScalarPolicy");       // Política de CORS para requisições cross-origin
app.UseErrorHandling();           // Tratamento global centralizado de erros
app.UseResponseCompression();     // Compressão GZIP das respostas HTTP
app.UseHttpsRedirection();        // Redirecionamento forçado para HTTPS
app.UseMiniProfiler();           // Coleta de métricas de performance
app.UseCorrelationId();          // Rastreamento distribuído de requisições
app.UseAuthentication();         // Verificação de identidade do usuário
app.UseAuthorization();          // Verificação de permissões de acesso
app.MapControllers();            // Mapeamento de rotas para controladores

#endregion

#region Application Startup

/// <summary>
/// Inicialização assíncrona da aplicação com todos os serviços
/// configurados e pipeline de middlewares estabelecido.
/// </summary>
await app.RunAppAsync();

#endregion

/*
🚀 EXPLICAÇÃO DETALHADA DO CÓDIGO CORE RESERVE API

📋 VISÃO GERAL:
O Program.cs representa o coração da aplicação Core Reserve API, implementando
uma arquitetura enterprise-ready com Clean Architecture, CQRS e múltiplas
camadas de observabilidade e segurança.

🏗️ ARQUITETURA IMPLEMENTADA:
- 🏛️ Clean Architecture: Separação clara de responsabilidades
- ⚡ CQRS: Command Query Responsibility Segregation
- 🗄️ Repository Pattern: Abstração de acesso a dados
- 💉 Dependency Injection: Inversão de controle nativa
- 🔄 Middleware Pattern: Pipeline de processamento de requisições

🔧 COMPONENTES PRINCIPAIS:

1️⃣ CONFIGURAÇÃO DE SERVIÇOS CORE:
   📦 Compressão GZIP para otimização de transferência
   🔄 Serialização JSON personalizada
   📍 Roteamento com URLs em minúsculas
   🌐 Cliente HTTP configurado
   📊 Compressão habilitada para HTTPS

2️⃣ VERSIONAMENTO DE API:
   📝 Suporte a múltiplas versões simultâneas
   📋 Documentação automática de versões
   🔗 Substituição de versão em URLs
   📊 Relatórios de versões disponíveis

3️⃣ DOCUMENTAÇÃO E SEGURANÇA:
   📚 OpenAPI/Swagger automático
   🔒 Proteção de dados integrada
   🎛️ Controladores MVC otimizados
   🌍 CORS diferenciado por ambiente

4️⃣ SERVIÇOS ESPECÍFICOS DA APLICAÇÃO:
   ⚙️ ConfigureAppSettings(): Configurações centralizadas
   🏗️ AddInfrastructure(): Serviços de infraestrutura
   📝 AddCommandHandlers(): Handlers de comando (CQRS)
   🔍 AddQueryHandlers(): Handlers de consulta (CQRS)
   💾 AddWriteDbContext(): Contexto otimizado para escrita
   📖 AddReadDbContext(): Contexto otimizado para leitura
   🗂️ AddRepositories(): Padrão Repository implementado
   ⚡ AddCacheService(): Sistema de cache distribuído
   🏥 AddHealthChecks(): Monitoramento de dependências
   🔗 AddCorrelationId(): Rastreamento distribuído
   🔐 AddJwtAuthentication(): Autenticação JWT Bearer

5️⃣ MONITORAMENTO E OBSERVABILIDADE:
   📊 MiniProfiler: Análise de performance em tempo real
   🏥 Health Checks: Monitoramento de saúde dos serviços
   🔗 Correlation ID: Rastreamento de requisições distribuídas
   📈 Entity Framework Profiling: Análise de queries SQL

6️⃣ CONFIGURAÇÕES DE SEGURANÇA:
   🛡️ Remoção do header Server (security through obscurity)
   🔒 Redirecionamento forçado para HTTPS
   🛡️ Proteção de dados criptográfica
   ✅ Validação de escopos em desenvolvimento
   🔐 Autenticação e autorização em camadas

7️⃣ PIPELINE DE MIDDLEWARES (ordem crítica):
   🌍 CORS Policy: Controle de requisições cross-origin
   🚨 Error Handling: Captura global de exceções
   📦 Response Compression: Otimização de transferência
   🔒 HTTPS Redirection: Força uso de protocolo seguro
   📊 MiniProfiler: Coleta de métricas
   🔗 Correlation ID: Rastreamento de requisições
   🔐 Authentication: Verificação de identidade
   ⚖️ Authorization: Controle de permissões
   🎯 Controllers: Roteamento final

8️⃣ FUNCIONALIDADES AVANÇADAS:
   📚 Scalar API Reference: Interface moderna de documentação
   ✅ FluentValidation: Sistema avançado de validação
   🌐 Validação i18n: Suporte a internacionalização
   🔧 Configuração condicional por ambiente

🎯 PADRÕES DE DESIGN UTILIZADOS:
- 💉 Dependency Injection Pattern
- 🏗️ Builder Pattern (configuração fluente)
- 🏭 Factory Pattern (criação de contextos)
- 🗄️ Repository Pattern (abstração de dados)
- ⚡ CQRS Pattern (separação comando/consulta)
- 🔄 Middleware Pattern (pipeline de processamento)
- ⚙️ Options Pattern (configurações tipadas)

🚀 BENEFÍCIOS DA ARQUITETURA:
- 📈 Escalabilidade: Contextos separados para leitura/escrita
- 🔧 Manutenibilidade: Responsabilidades bem definidas
- 🧪 Testabilidade: DI facilita criação de testes
- 👁️ Observabilidade: Monitoramento completo da aplicação
- ⚡ Performance: Cache, compressão e profiling integrados
- 🛡️ Segurança: Múltiplas camadas de proteção
- 📚 Documentação: Geração automática e interativa
- 🔄 Confiabilidade: Health checks e correlation tracking

💡 CASOS DE USO:
- APIs empresariais de alta escala
- Sistemas com requisitos rigorosos de observabilidade
- Aplicações que necessitam versionamento de API
- Sistemas com separação clara de leitura/escrita
- Aplicações que requerem documentação automática
- APIs com necessidades de cache distribuído

Esta configuração representa o estado da arte em aplicações .NET modernas,
implementando todas as práticas recomendadas para sistemas enterprise em produção.
*/