using System.Linq;
using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using CoreReserve.PublicApi.Models;





namespace CoreReserve.PublicApi.Extensions
{
    /// <summary>
    /// Classe de extensão para conversão de objetos <see cref="Result"/> em respostas HTTP.
    /// Facilita a transformação de resultados em <see cref="IActionResult"/> para utilização em APIs.
    /// </summary>
    internal static class ResultExtensions
    {
        /// <summary>
        /// Converte um objeto <see cref="Result"/> em um <see cref="IActionResult"/>.
        /// Se o resultado for um sucesso, retorna um status 200 OK; caso contrário, converte para um erro HTTP adequado.
        /// </summary>
        /// <param name="result">Objeto <see cref="Result"/> a ser convertido.</param>
        /// <returns>Instância de <see cref="IActionResult"/> correspondente ao resultado.</returns>
        public static IActionResult ToActionResult(this Result result) =>
            result.IsSuccess
                ? new OkObjectResult(ApiResponse.Ok(result.SuccessMessage))
                : result.ToHttpNonSuccessResult();

        /// <summary>
        /// Converte um <see cref="Result{T}"/> em um <see cref="IActionResult"/>.
        /// Define automaticamente o status HTTP com base no sucesso ou falha do resultado.
        /// </summary>
        /// <typeparam name="T">Tipo do valor do resultado.</typeparam>
        /// <param name="result">Resultado a ser convertido.</param>
        /// <returns>Instância de <see cref="IActionResult"/> correspondente ao resultado.</returns>
        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            if (result.IsCreated())
            {
                return new CreatedResult(result.Location, ApiResponse<T>.Created(result.Value));
            }
            else if (result.IsOk())
            {
                return new OkObjectResult(ApiResponse<T>.Ok(result.Value, result.SuccessMessage));
            }
            else
            {
                return result.ToHttpNonSuccessResult();
            }
        }

        /// <summary>
        /// Converte um objeto <see cref="IResult"/> com status de erro em um <see cref="IActionResult"/> apropriado.
        /// </summary>
        /// <param name="result">Objeto <see cref="IResult"/> contendo informações sobre o erro.</param>
        /// <returns>Instância de <see cref="IActionResult"/> correspondente ao erro.</returns>
        private static IActionResult ToHttpNonSuccessResult(this Ardalis.Result.IResult result)
        {
            var errors = result.Errors.Select(error => new ApiErrorResponse(error)).ToList();


            switch (result.Status)
            {
                case ResultStatus.Invalid:
                    var validationErrors = result.ValidationErrors
                        .Select(validation => new ApiErrorResponse(validation.ErrorMessage));
                    return new BadRequestObjectResult(ApiResponse.BadRequest(validationErrors));

                case ResultStatus.NotFound:
                    return new NotFoundObjectResult(ApiResponse.NotFound(result.Errors.First()));

                case ResultStatus.Forbidden:
                    return new ForbidResult();

                case ResultStatus.Unauthorized:
                    return new UnauthorizedObjectResult(ApiResponse.Unauthorized(errors));

                default:
                    return new BadRequestObjectResult(ApiResponse.BadRequest(errors));
            }
        }
    }

}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe ResultExtensions → Adiciona métodos de extensão para conversão de `Result` em respostas HTTP.
✅ Método ToActionResult() → Transforma um resultado simples em uma resposta HTTP apropriada.
✅ Método ToActionResult<T>() → Converte um resultado contendo dados (genérico) em uma resposta HTTP.
✅ Método ToHttpNonSuccessResult() → Interpreta erros e retorna respostas HTTP específicas:
   - `ResultStatus.Invalid` → Retorna `400 Bad Request` com erros de validação.
   - `ResultStatus.NotFound` → Retorna `404 Not Found`.
   - `ResultStatus.Forbidden` → Retorna `403 Forbidden`.
   - `ResultStatus.Unauthorized` → Retorna `401 Unauthorized`.
   - Outros erros → Retornam `400 Bad Request`.
✅ Uso de `IActionResult` → Garante compatibilidade com APIs ASP.NET Core.
✅ Uso de `ApiResponse` → Mantém consistência e padronização nas respostas da API.
✅ Essa abordagem melhora a estrutura de código, facilita manutenção e assegura respostas adequadas para requisições da API.
*/