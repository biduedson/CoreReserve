using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
namespace CoreReserve.PublicApi.Models
{
    /// <summary>
    /// Representa uma resposta padronizada para a API.
    /// Contém informações sobre o sucesso da requisição, status HTTP e possíveis mensagens de erro.
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Construtor para desserialização JSON.
        /// Permite que a resposta seja recriada a partir de um formato serializado.
        /// </summary>
        /// <param name="success">Indica se a requisição foi bem-sucedida.</param>
        /// <param name="successMessage">Mensagem de sucesso, se aplicável.</param>
        /// <param name="statusCode">Código HTTP correspondente ao status da requisição.</param>
        /// <param name="errors">Lista de mensagens de erro, se houver falhas na requisição.</param>
        [JsonConstructor]
        public ApiResponse(bool success, string successMessage, int statusCode, IEnumerable<ApiErrorResponse> errors)
        {
            Success = success;
            SuccessMessage = successMessage;
            StatusCode = statusCode;
            Errors = errors;
        }

        /// <summary>
        /// Construtor padrão para inicialização sem parâmetros.
        /// </summary>
        public ApiResponse()
        {
        }

        /// <summary>
        /// Indica se a requisição foi bem-sucedida.
        /// </summary>
        public bool Success { get; protected init; }

        /// <summary>
        /// Mensagem de sucesso associada à resposta, caso aplicável.
        /// </summary>
        public string SuccessMessage { get; protected init; }

        /// <summary>
        /// Código de status HTTP da resposta.
        /// </summary>
        public int StatusCode { get; protected init; }

        /// <summary>
        /// Lista de mensagens de erro, caso a requisição falhe.
        /// </summary>
        public IEnumerable<ApiErrorResponse> Errors { get; private init; } = [];

        // -----------------------------------------
        // 🔹 Métodos Estáticos para Criar Respostas 🔹
        // -----------------------------------------

        /// <summary>
        /// Retorna uma resposta de sucesso (200 OK) sem mensagem adicional.
        /// </summary>
        public static ApiResponse Ok() =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK };

        /// <summary>
        /// Retorna uma resposta de sucesso (200 OK) com uma mensagem.
        /// </summary>
        public static ApiResponse Ok(string successMessage) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, SuccessMessage = successMessage };

        /// <summary>
        /// Retorna uma resposta de erro (400 Bad Request) sem detalhes adicionais.
        /// </summary>
        public static ApiResponse BadRequest() =>
            new() { Success = false, StatusCode = StatusCodes.Status400BadRequest };

        /// <summary>
        /// Retorna uma resposta de erro (400 Bad Request) com mensagem de erro.
        /// </summary>
        public static ApiResponse BadRequest(string errorMessage) =>
            new() { Success = false, StatusCode = StatusCodes.Status400BadRequest, Errors = CreateErrors(errorMessage) };

        /// <summary>
        /// Retorna uma resposta de erro (400 Bad Request) com lista de erros.
        /// </summary>
        public static ApiResponse BadRequest(IEnumerable<ApiErrorResponse> errors) =>
            new() { Success = false, StatusCode = StatusCodes.Status400BadRequest, Errors = errors };

        /// <summary>
        /// Retorna uma resposta de erro (401 Unauthorized).
        /// </summary>
        public static ApiResponse Unauthorized() =>
            new() { Success = false, StatusCode = StatusCodes.Status401Unauthorized };

        /// <summary>
        /// Retorna uma resposta de erro (401 Unauthorized) com mensagem de erro.
        /// </summary>
        public static ApiResponse Unauthorized(string errorMessage) =>
            new() { Success = false, StatusCode = StatusCodes.Status401Unauthorized, Errors = CreateErrors(errorMessage) };

        /// <summary>
        /// Retorna uma resposta de erro (401 Unauthorized) com lista de erros.
        /// </summary>
        public static ApiResponse Unauthorized(IEnumerable<ApiErrorResponse> errors) =>
            new() { Success = false, StatusCode = StatusCodes.Status401Unauthorized, Errors = errors };

        /// <summary>
        /// Retorna uma resposta de erro (403 Forbidden).
        /// </summary>
        public static ApiResponse Forbidden() =>
            new() { Success = false, StatusCode = StatusCodes.Status403Forbidden };

        /// <summary>
        /// Retorna uma resposta de erro (403 Forbidden) com mensagem de erro.
        /// </summary>
        public static ApiResponse Forbidden(string errorMessage) =>
            new() { Success = false, StatusCode = StatusCodes.Status403Forbidden, Errors = CreateErrors(errorMessage) };

        /// <summary>
        /// Retorna uma resposta de erro (404 Not Found).
        /// </summary>
        public static ApiResponse NotFound() =>
            new() { Success = false, StatusCode = StatusCodes.Status404NotFound };

        /// <summary>
        /// Retorna uma resposta de erro (404 Not Found) com mensagem de erro.
        /// </summary>
        public static ApiResponse NotFound(string errorMessage) =>
            new() { Success = false, StatusCode = StatusCodes.Status404NotFound, Errors = CreateErrors(errorMessage) };

        /// <summary>
        /// Retorna uma resposta de erro (500 Internal Server Error) com mensagem de erro.
        /// </summary>
        public static ApiResponse InternalServerError(string errorMessage) =>
            new() { Success = false, StatusCode = StatusCodes.Status500InternalServerError, Errors = CreateErrors(errorMessage) };

        /// <summary>
        /// Cria uma lista de erros a partir de uma mensagem simples.
        /// </summary>
        /// <param name="errorMessage">Mensagem de erro.</param>
        private static ApiErrorResponse[] CreateErrors(string errorMessage) =>
            [new ApiErrorResponse(errorMessage)];


        /// <summary>
        /// Retorna uma representação textual da resposta, útil para logs e debugging.
        /// </summary>
        public override string ToString() =>
            $"Success: {Success} | StatusCode: {StatusCode} | HasErrors: {Errors.Any()}";
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe ApiResponse → Define um modelo padronizado de resposta para a API.
✅ Uso de [JsonConstructor] → Permite que a classe seja corretamente desserializada ao receber JSON.
✅ Métodos estáticos → Facilitam a criação de respostas comuns como 200 OK, 400 Bad Request, 404 Not Found e 500 Internal Server Error.
✅ Uso de IEnumerable<ApiErrorResponse> → Suporta múltiplas mensagens de erro em uma única resposta.
✅ Método CreateErrors() → Padroniza a conversão de mensagens simples para objetos de erro.
✅ Método ToString() → Retorna uma descrição legível do estado da resposta, facilitando logs e debugging.
✅ Arquitetura baseada em respostas padronizadas → Garante consistência e organização na API.
✅ Essa abordagem melhora a comunicação entre API e cliente, tornando o sistema mais confiável.
*/
