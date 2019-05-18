using System.Net.Http;
using System.Threading.Tasks;

namespace AppFramework.Core.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<T> ContentToAsync<T>(this HttpResponseMessage response)
        {
            var data = await response.Content?.ReadAsStringAsync();
            if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(data))
                return Serializer.Deserialize<T>(data);
            else
                return default(T);
        }
    }
}