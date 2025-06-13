
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using CoreReserve.Core.Extensions;

namespace CoreReserve.IntegrationTests.Extensions
{
    public static class TRequestExtensions
    {
        public static HttpContent ToJsonHttpContent<TRequest>(this TRequest request) =>
            new StringContent(request.ToJson(), Encoding.UTF8, MediaTypeNames.Application.Json);
    }
}