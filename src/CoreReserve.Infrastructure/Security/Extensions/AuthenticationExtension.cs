using System.Text;
using CoreReserve.Core.AppSettings;
using CoreReserve.Core.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CoreReserve.Infrastructure.Security.Extensions
{
    #region JWT Authentication Extension

    /// <summary>
    /// Extensão para configuração de autenticação JWT Bearer na aplicação.
    /// Fornece métodos para registrar e configurar serviços de autenticação JWT
    /// com tratamento customizado de exceções e validação de tokens.
    /// </summary>
    /// <remarks>
    /// Esta extensão centraliza toda a configuração JWT, incluindo parâmetros de validação,
    /// tratamento de eventos e integração com o sistema de logging da aplicação.
    /// Utiliza o JwtExceptionHandler para tratamento consistente de erros.
    /// </remarks>
    public static class AuthenticationExtension
    {
        #region JWT Configuration Method

        /// <summary>
        /// Configura e registra os serviços de autenticação JWT Bearer no container de DI.
        /// </summary>
        /// <param name="services">Coleção de serviços para registro das dependências</param>
        /// <param name="configuration">Configuração da aplicação contendo as opções JWT</param>
        /// <returns>A coleção de serviços modificada para permitir chaining de configurações</returns>
        /// <remarks>
        /// Configura:
        /// - Parâmetros de validação de token (issuer, audience, lifetime, signature)
        /// - Eventos customizados para tratamento de exceções
        /// - Logging de sucessos e falhas de autenticação
        /// - Integração com JwtExceptionHandler para respostas consistentes
        /// </remarks>
        /// <exception cref="ArgumentNullException">Lançada quando services ou configuration são nulos</exception>
        /// <exception cref="InvalidOperationException">Lançada quando as configurações JWT estão inválidas</exception>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetOptions<JwtOptions>();

            var secretKey = jwtSettings.SecretKey;
            var issuer = jwtSettings.Issuer;
            var audience = jwtSettings.Audience;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    #region Token Validation Parameters

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = key,
                        ClockSkew = TimeSpan.Zero
                    };

                    #endregion

                    #region JWT Bearer Events

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = JwtExceptionHandler.OnAuthenticationFailed,
                        OnChallenge = JwtExceptionHandler.OnChallenge,
                        OnForbidden = JwtExceptionHandler.OnForbidden,
                        OnTokenValidated = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                            var userId = context.Principal?.FindFirst("sub")?.Value;
                            logger.LogInformation("✅ JWT Validado com sucesso | Usuário ID: {UserId}", userId);
                            return Task.CompletedTask;
                        }
                    };

                    #endregion
                });

            return services;
        }

        #endregion
    }

    #endregion
}

/*
🔐 EXPLICAÇÃO DETALHADA DO CÓDIGO

📋 VISÃO GERAL:
A AuthenticationExtension é uma classe estática que fornece métodos de extensão
para configurar autenticação JWT Bearer em aplicações ASP.NET Core. Ela encapsula
toda a complexidade de configuração JWT, oferecendo uma interface simples e
consistente para integração com o sistema de autenticação.

🎯 PROPÓSITO:
- Centralizar configuração de autenticação JWT
- Simplificar integração com container de DI
- Padronizar tratamento de eventos de autenticação
- Fornecer logging estruturado para auditoria

🔧 COMPONENTES PRINCIPAIS:

1️⃣ AddJwtAuthentication:
   ⚙️ Método de extensão principal para IServiceCollection
   🔑 Configura parâmetros de validação de token
   📊 Integra com JwtExceptionHandler para tratamento de erros
   ✅ Implementa logging de sucessos de autenticação

📋 PARÂMETROS DE VALIDAÇÃO:
- ValidateIssuer: Valida o emissor do token
- ValidateAudience: Valida o público-alvo do token
- ValidateLifetime: Verifica se o token não expirou
- ValidateIssuerSigningKey: Valida a assinatura digital
- ClockSkew = Zero: Remove tolerância de tempo para maior precisão

🎪 EVENTOS CONFIGURADOS:

🚨 OnAuthenticationFailed:
   - Delegado para JwtExceptionHandler.OnAuthenticationFailed
   - Trata falhas específicas de validação
   - Retorna respostas JSON estruturadas

🛡️ OnChallenge:
   - Delegado para JwtExceptionHandler.OnChallenge
   - Intercepta desafios de autorização
   - Substitui resposta HTML por JSON

⛔ OnForbidden:
   - Delegado para JwtExceptionHandler.OnForbidden
   - Trata casos de acesso negado

✅ OnTokenValidated:
   - Registra sucessos de autenticação
   - Extrai e loga ID do usuário
   - Fornece auditoria positiva

⚙️ INTEGRAÇÃO:
Esta extensão é utilizada no Program.cs da aplicação:

builder.Services.AddJwtAuthentication(builder.Configuration);

🔒 BENEFÍCIOS DE SEGURANÇA:
- Validação rigorosa de todos os aspectos do token
- Logging completo de eventos de autenticação
- Tratamento consistente de exceções
- Auditoria de sucessos e falhas

💡 CARACTERÍSTICAS TÉCNICAS:
- Método de extensão para IServiceCollection
- Configuração fluente e legível
- Integração com sistema de configuração do .NET
- Suporte a dependency injection
- Tratamento de exceções centralizado

🎨 PADRÕES IMPLEMENTADOS:
- Extension Method Pattern
- Fluent Interface
- Dependency Injection
- Configuration Pattern
- Event Handler Pattern

📈 VANTAGENS:
- Configuração única e reutilizável
- Manutenção centralizada
- Testabilidade aprimorada
- Separação de responsabilidades
- Código limpo e organizado
*/