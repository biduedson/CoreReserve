using System.Net;
using FluentAssertions;
namespace CoreReserve.IntegrationTests.Abstractions
{
    /// <summary>
    /// Validações específicas para códigos de status HTTP
    /// </summary>
    public static class HttpStatusAssertion
    {
        public static void AssertCreated(HttpResponseMessage response)
        {
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        public static void AssertBadRequest(HttpResponseMessage response)
        {
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.IsSuccessStatusCode.Should().BeFalse();
        }

        public static void AssertNotFound(HttpResponseMessage response)
        {
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.IsSuccessStatusCode.Should().BeFalse();
        }

        public static void AssertUnauthorized(HttpResponseMessage response)
        {
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.IsSuccessStatusCode.Should().BeFalse();
        }

        public static void AssertOk(HttpResponseMessage response)
        {
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}

