using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using CoreReserve.Application.User.Responses;
using System.Text.Json;
namespace CoreReserve.IntegrationTests.Abstractions
{
    /// <summary>
    /// Classe responsável por todas as validações relacionadas às respostas de User
    /// </summary>
    public class UserResponseAssertion : BaseResponseAssertion
    {
        /// <summary>
        /// Valida resposta de sucesso na criação de usuário
        /// </summary>
        public static async Task AssertSuccessfulUserCreation(HttpResponseMessage response)
        {
            // Validações HTTP básicas
            AssertBasicHttpResponse(response, HttpStatusCode.Created);
            response.IsSuccessStatusCode.Should().BeTrue();

            // Validações do conteúdo da resposta
            var apiResponse = await DeserializeResponse<CreatedUserResponse>(response);

            AssertApiResponseStructure(apiResponse, shouldBeSuccess: true);
            apiResponse.StatusCode.Should().Be(StatusCodes.Status201Created);
            apiResponse.Errors.Should().BeEmpty();

            // Validações específicas do usuário criado
            AssertCreatedUserDetails(apiResponse.Result);

            // Validações de headers REST
            AssertLocationHeader(response, apiResponse.Result.Id);
        }

        /// <summary>
        /// Valida resposta de falha na validação de dados
        /// </summary>
        public static async Task AssertValidationFailure(HttpResponseMessage response)
        {
            // Validações HTTP básicas
            AssertBasicHttpResponse(response, HttpStatusCode.BadRequest);
            response.IsSuccessStatusCode.Should().BeFalse();

            // Validações do conteúdo da resposta
            var apiResponse = await DeserializeResponse<CreatedUserResponse>(response);

            AssertApiResponseStructure(apiResponse, shouldBeSuccess: false);
            apiResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            apiResponse.Result.Should().BeNull();

            // Validações dos erros
            AssertValidationErrors(apiResponse.Errors);
        }

        /// <summary>
        /// Valida erro específico de email duplicado
        /// </summary>
        public static async Task AssertDuplicateEmailError(HttpResponseMessage response)
        {
            // Validações HTTP básicas
            AssertBasicHttpResponse(response, HttpStatusCode.BadRequest);
            response.IsSuccessStatusCode.Should().BeFalse();

            // Validações do conteúdo da resposta
            var apiResponse = await DeserializeResponse<CreatedUserResponse>(response);

            AssertApiResponseStructure(apiResponse, shouldBeSuccess: false);
            apiResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            apiResponse.Result.Should().BeNull();

            // Validação específica do erro de email duplicado
            AssertDuplicateEmailErrorMessage(apiResponse.Errors);
        }

        #region Private Helper Methods

        /// <summary>
        /// Valida detalhes do usuário criado
        /// </summary>
        private static void AssertCreatedUserDetails(CreatedUserResponse userResponse)
        {
            userResponse.Should().NotBeNull();
            userResponse.Id.Should().NotBeEmpty();
            // Adicione outras validações específicas do usuário conforme necessário
        }

        /// <summary>
        /// Valida header Location (padrão REST)
        /// </summary>
        private static void AssertLocationHeader(HttpResponseMessage response, Guid userId)
        {
            response.Headers.GetValues("Location")
                .Should().NotBeNullOrEmpty()
                .And.Contain($"/api/users/{userId}");
        }

        /// <summary>
        /// Valida estrutura geral dos erros de validação
        /// </summary>
        private static void AssertValidationErrors(IEnumerable<object> errors)
        {
            errors.Should().NotBeNullOrEmpty()
                .And.OnlyHaveUniqueItems();
        }

        /// <summary>
        /// Valida mensagem específica de erro de email duplicado
        /// </summary>
        private static void AssertDuplicateEmailErrorMessage(IEnumerable<object> errors)
        {
            errors.Should().NotBeNullOrEmpty()
                .And.OnlyHaveUniqueItems();

            // Converte toda a coleção de erros para string JSON e verifica se contém a mensagem esperada
            var errorsJson = JsonSerializer.Serialize(errors);
            errorsJson.Should().Contain("The provided email address is already in use.");
        }

        #endregion
    }
}