using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using CoreReserve.Core.Extensions;
using CoreReserve.PublicApi.Models;
namespace CoreReserve.IntegrationTests.Abstractions
{
    /// <summary>
    /// Classe base para todas as validações de resposta HTTP
    /// </summary>
    public abstract class BaseResponseAssertion
    {
        /// <summary>
        /// Deserializa a resposta HTTP para o tipo especificado
        /// </summary>
        protected static async Task<ApiResponse<T>> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent.FromJson<ApiResponse<T>>();
        }

        /// <summary>
        /// Valida os aspectos básicos de uma resposta HTTP
        /// </summary>
        protected static void AssertBasicHttpResponse(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
        {
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(expectedStatusCode);
        }

        /// <summary>
        /// Valida estrutura básica de uma ApiResponse
        /// </summary>
        protected static void AssertApiResponseStructure<T>(ApiResponse<T> apiResponse, bool shouldBeSuccess)
        {
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().Be(shouldBeSuccess);
        }
    }
}