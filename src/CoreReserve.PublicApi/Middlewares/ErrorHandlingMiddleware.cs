using System.Net.Mime;
using CoreReserve.Core.Extensions;
using CoreReserve.PublicApi.Models;

namespace CoreReserve.PublicApi.Middlewares
{
    /// <summary>
    /// Middleware responsável por tratar erros globais na API.
    /// Intercepta exceções não tratadas e retorna uma resposta consistente ao cliente.
    /// </summary>
    public class ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        /// <summary>
        /// Mensagem padrão para erros internos do servidor.
        /// </summary>
        private const string ErrorMessage = "An internal error occurred while processing your request.";

        /// <summary>
        /// Resposta padronizada em formato JSON para erros internos.
        /// </summary>
        private static readonly string ApiResponseJson = ApiResponse.InternalServerError(ErrorMessage).ToJson();

        /// <summary>
        /// Intercepta requisições, executa o próximo middleware e captura exceções inesperadas.
        /// </summary>
        /// <param name="httpContext">Contexto da requisição.</param>
        /// <returns>Tarefa assíncrona representando a execução do middleware.</returns>
        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                // Passa a requisição para o próximo middleware da pipeline.
                await next(httpContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected exception was thrown: {Message}", ex.Message);

                // Define o código de status HTTP como erro interno do servidor (500).
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                // Se estiver em ambiente de desenvolvimento, retorna detalhes da exceção como texto.
                if (environment.IsDevelopment())
                {
                    httpContext.Response.ContentType = MediaTypeNames.Text.Plain;
                    await httpContext.Response.WriteAsync(ex.ToString());
                }
                else
                {
                    // Em ambiente de produção, retorna uma mensagem genérica em JSON.
                    httpContext.Response.ContentType = MediaTypeNames.Application.Json;
                    await httpContext.Response.WriteAsync(ApiResponseJson);
                }
            }
        }
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe ErrorHandlingMiddleware → Middleware global para captura de erros na API.
✅ Uso de RequestDelegate → Permite que o middleware execute o próximo na cadeia antes de capturar erros.
✅ Uso de ILogger → Loga erros inesperados para diagnóstico e auditoria.
✅ Uso de IHostEnvironment → Diferencia ambientes de desenvolvimento e produção.
✅ Comportamento da resposta:
   🔹 Em **desenvolvimento**, exibe detalhes completos da exceção em formato de texto.
   🔹 Em **produção**, retorna uma mensagem genérica em JSON para evitar exposição de informações sensíveis.
✅ Método Invoke() → Executa a requisição e captura exceções, garantindo um tratamento global de erros.
✅ Arquitetura baseada em Middlewares → Modulariza o tratamento de erros e evita duplicação de código.
✅ Essa abordagem melhora a estabilidade da API e facilita a manutenção de logs e rastreamento de erros.
*/
