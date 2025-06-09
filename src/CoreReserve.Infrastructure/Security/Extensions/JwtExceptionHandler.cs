using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mime;
using System.Text.Json;

namespace CoreReserve.Infrastructure.Security.Extensions
{
    #region JWT Exception Handler

    /// <summary>
    /// Manipulador de exceções para autenticação JWT Bearer.
    /// Fornece tratamento customizado para falhas de autenticação, desafios de autorização e proibições de acesso.
    /// </summary>
    /// <remarks>
    /// Esta classe centraliza o tratamento de erros JWT, garantindo respostas consistentes em formato JSON
    /// e logging adequado para monitoramento e auditoria de segurança.
    /// </remarks>
    internal sealed class JwtExceptionHandler
    {
        #region Authentication Failed Handler

        /// <summary>
        /// Manipula falhas de autenticação JWT, fornecendo respostas JSON estruturadas
        /// e logging detalhado para diferentes tipos de exceções de token.
        /// </summary>
        /// <param name="context">Contexto da falha de autenticação contendo informações sobre o erro</param>
        /// <returns>Task representando a operação assíncrona de escrita da resposta</returns>
        /// <remarks>
        /// Trata especificamente:
        /// - Tokens expirados
        /// - Assinaturas inválidas
        /// - Issuer/Audience inválidos
        /// - Problemas de lifetime
        /// - Tokens malformados
        /// </remarks>
        public static Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
            var response = context.Response;
            response.StatusCode = StatusCodes.Status401Unauthorized;
            response.ContentType = MediaTypeNames.Application.Json;

            string message = context.Exception switch
            {
                SecurityTokenExpiredException => "Token expirado.",
                SecurityTokenInvalidSignatureException => "Assinatura do token inválida.",
                SecurityTokenInvalidIssuerException => "Issuer inválido.",
                SecurityTokenInvalidAudienceException => "Audience inválido.",
                SecurityTokenNoExpirationException => "Token sem expiração.",
                SecurityTokenInvalidLifetimeException => "Lifetime inválido.",
                ArgumentException => "Token ausente ou malformado.",
                _ => "Falha na autenticação com o token."
            };

            logger.LogWarning("🔐 JWT Authentication Failed | IP: {IP} | Erro: {ErroDetalhado} | Exception: {Exception}",
                context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                message,
                context.Exception?.Message);

            var json = JsonSerializer.Serialize(new
            {
                statusCode = 401,
                message,
                error = context.Exception?.GetType().Name
            });

            return response.WriteAsync(json);
        }

        #endregion

        #region Challenge Handler

        /// <summary>
        /// Manipula desafios de autorização JWT quando o token está ausente ou não é válido.
        /// Substitui a resposta HTML padrão por uma resposta JSON estruturada.
        /// </summary>
        /// <param name="context">Contexto do desafio de autorização</param>
        /// <returns>Task representando a operação assíncrona de escrita da resposta</returns>
        /// <remarks>
        /// Intercepta o comportamento padrão do ASP.NET Core que retornaria uma página HTML 401,
        /// fornecendo uma resposta JSON consistente para APIs.
        /// </remarks>
        public static Task OnChallenge(JwtBearerChallengeContext context)
        {
            context.HandleResponse(); // Impede o comportamento padrão (401 HTML)
            var response = context.Response;
            response.StatusCode = StatusCodes.Status401Unauthorized;
            response.ContentType = MediaTypeNames.Application.Json;

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
            const string message = "Token ausente ou não autorizado.";

            logger.LogWarning("🚫 JWT Challenge | IP: {IP} | Detalhe: {Detalhe}",
                context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                message);

            var json = JsonSerializer.Serialize(new
            {
                statusCode = 401,
                message
            });

            return response.WriteAsync(json);
        }

        #endregion

        #region Forbidden Handler

        /// <summary>
        /// Manipula situações onde o usuário está autenticado mas não possui permissão
        /// para acessar o recurso solicitado.
        /// </summary>
        /// <param name="context">Contexto da proibição de acesso</param>
        /// <returns>Task representando a operação assíncrona de escrita da resposta</returns>
        /// <remarks>
        /// Diferencia entre não autenticado (401) e não autorizado (403),
        /// fornecendo feedback claro sobre a natureza da restrição de acesso.
        /// </remarks>
        public static Task OnForbidden(ForbiddenContext context)
        {
            var response = context.Response;
            response.StatusCode = StatusCodes.Status403Forbidden;
            response.ContentType = MediaTypeNames.Application.Json;

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

            logger.LogWarning("⛔ JWT Forbidden | IP: {IP} | Recurso acessado sem permissão",
                context.HttpContext.Connection.RemoteIpAddress?.ToString());

            var json = JsonSerializer.Serialize(new
            {
                statusCode = 403,
                message = "Acesso negado."
            });

            return response.WriteAsync(json);
        }

        #endregion
    }

    #endregion
}

/*
🔐 EXPLICAÇÃO DETALHADA DO CÓDIGO

📋 VISÃO GERAL:
O JwtExceptionHandler é uma classe utilitária que centraliza o tratamento de exceções 
relacionadas à autenticação JWT Bearer em aplicações ASP.NET Core. Ela substitui o 
comportamento padrão do framework, fornecendo respostas JSON consistentes e logging 
estruturado para eventos de segurança.

🎯 PROPÓSITO:
- Padronizar respostas de erro de autenticação em formato JSON
- Fornecer logging detalhado para auditoria e monitoramento
- Melhorar a experiência do desenvolvedor com mensagens de erro claras
- Manter consistência na API de resposta

🔧 COMPONENTES PRINCIPAIS:

1️⃣ OnAuthenticationFailed:
   🚨 Trata falhas específicas de validação de token
   📝 Mapeia diferentes tipos de exceção para mensagens user-friendly
   📊 Registra logs com IP e detalhes da falha
   🔄 Retorna JSON estruturado com código 401

2️⃣ OnChallenge:
   🛡️ Intercepta desafios de autorização
   🚫 Substitui resposta HTML padrão por JSON
   📍 Usado quando token está ausente ou inválido
   🔍 Fornece feedback claro sobre necessidade de autenticação

3️⃣ OnForbidden:
   ⛔ Trata casos de acesso negado (usuário autenticado mas sem permissão)
   🎯 Diferencia entre não autenticado (401) e não autorizado (403)
   📋 Mantém consistência com outras respostas da API

🔒 BENEFÍCIOS DE SEGURANÇA:
- Logging de tentativas de acesso não autorizadas
- Rastreamento de IPs para análise de segurança
- Mensagens de erro que não expõem informações sensíveis
- Auditoria completa de eventos de autenticação

⚙️ INTEGRAÇÃO:
Esta classe é utilizada internamente pela AuthenticationExtension para configurar 
automaticamente o tratamento de exceções JWT durante a configuração dos serviços 
de autenticação da aplicação.

🎨 CARACTERÍSTICAS TÉCNICAS:
- Métodos estáticos para performance
- Respostas assíncronas para não bloquear threads
- Serialização JSON nativa do .NET
- Logging estruturado com templates
- Sealed class para prevenção de herança desnecessária

💡 CASOS DE USO:
- APIs REST que precisam de respostas consistentes
- Aplicações com requisitos de auditoria
- Sistemas com múltiplos clientes (web, mobile, desktop)
- Ambientes onde debugging de autenticação é crítico
*/