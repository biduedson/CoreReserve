using CoreReserve.PublicApi.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace CoreReserve.PublicApi.Extensions
{
    /// <summary>
    /// Classe de extensão para registrar Middlewares personalizados no pipeline da aplicação.
    /// Facilita a adição de funcionalidades globais, como tratamento de erros.
    /// </summary>
    internal static class MiddlewareExtensions
    {
        /// <summary>
        /// Adiciona o middleware de tratamento global de erros ao pipeline da aplicação.
        /// Garante que exceções sejam capturadas e respondidas de forma padronizada.
        /// </summary>
        /// <param name="builder">Instância do <see cref="IApplicationBuilder"/> utilizada na configuração do middleware.</param>
        public static void UseErrorHandling(this IApplicationBuilder builder) =>
            builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe MiddlewareExtensions → Define métodos de extensão para adicionar Middlewares personalizados à aplicação.
✅ Método UseErrorHandling() → Registra o middleware de tratamento de erros globalmente no pipeline de requisição.
✅ Uso de builder.UseMiddleware<T>() → Injeta o `ErrorHandlingMiddleware` automaticamente na execução de cada requisição.
✅ Arquitetura baseada em Middlewares → Modulariza o tratamento de erros e facilita manutenção sem duplicação de código.
✅ Essa abordagem melhora a robustez da API, padroniza respostas e garante melhor experiência para clientes.
*/
