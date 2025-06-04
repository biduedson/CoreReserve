using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace CoreReserve.PublicApi.Models
{
    /// <summary>
    /// Representa uma resposta padronizada da API contendo um resultado.
    /// Estende <see cref="ApiResponse"/> para incluir um objeto do tipo <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">Tipo do resultado retornado pela API.</typeparam>
    public sealed class ApiResponse<TResult> : ApiResponse
    {
        /// <summary>
        /// Construtor utilizado para desserialização JSON.
        /// Permite que a resposta seja recriada a partir de um formato serializado.
        /// </summary>
        /// <param name="result">Objeto de resultado retornado pela API.</param>
        /// <param name="success">Indica se a requisição foi bem-sucedida.</param>
        /// <param name="successMessage">Mensagem de sucesso, se aplicável.</param>
        /// <param name="statusCode">Código HTTP correspondente ao status da requisição.</param>
        /// <param name="errors">Lista de mensagens de erro, caso existam.</param>
        [JsonConstructor]
        public ApiResponse(
            TResult result,
            bool success,
            string successMessage,
            int statusCode,
            IEnumerable<ApiErrorResponse> errors) : base(success, successMessage, statusCode, errors)
        {
            Result = result;
        }

        /// <summary>
        /// Construtor padrão para inicialização sem parâmetros.
        /// </summary>
        public ApiResponse()
        {
        }

        /// <summary>
        /// Objeto retornado na resposta da API.
        /// </summary>
        public TResult Result { get; private init; }

        // -----------------------------------------
        // 🔹 Métodos Estáticos para Criar Respostas 🔹
        // -----------------------------------------

        /// <summary>
        /// Retorna uma resposta de sucesso (200 OK) contendo o resultado especificado.
        /// </summary>
        /// <param name="result">Objeto de resultado.</param>
        public static ApiResponse<TResult> Ok(TResult result) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, Result = result };

        /// <summary>
        /// Retorna uma resposta de sucesso (200 OK) contendo o resultado e uma mensagem adicional.
        /// </summary>
        /// <param name="result">Objeto de resultado.</param>
        /// <param name="successMessage">Mensagem de sucesso.</param>
        public static ApiResponse<TResult> Ok(TResult result, string successMessage) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, Result = result, SuccessMessage = successMessage };

        /// <summary>
        /// Retorna uma resposta de sucesso (201 Created) contendo o resultado especificado.
        /// </summary>
        /// <param name="result">Objeto de resultado.</param>
        public static ApiResponse<TResult> Created(TResult result) =>
            new() { Success = true, StatusCode = StatusCodes.Status201Created, Result = result };
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe ApiResponse<TResult> → Define um modelo padronizado de resposta para a API que inclui um resultado.
✅ Herança de ApiResponse → Reutiliza estrutura de resposta padrão, adicionando um objeto de resultado.
✅ Uso de [JsonConstructor] → Permite que a classe seja corretamente desserializada ao receber JSON.
✅ Propriedade Result → Armazena o objeto retornado pela API, garantindo que a resposta inclua dados relevantes.
✅ Métodos estáticos → Facilitam a criação de respostas comuns como 200 OK e 201 Created.
✅ Arquitetura baseada em respostas padronizadas → Garante consistência na comunicação entre API e cliente.
✅ Essa abordagem melhora a integridade dos dados e facilita a manutenção do sistema.
*/
