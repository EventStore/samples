using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;

namespace Core.Testing
{
    public static class ResponseExtensions
    {
        public static async Task<T> GetResultFromJson<T>(this HttpResponseMessage response)
        {
            var result = await response.Content.ReadAsStringAsync();

            result.Should().NotBeNull();
            result.Should().NotBe(string.Empty);

            return result.FromJson<T>();
        }
    }
}
