using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightInformationApi.Tests;

/// <summary>Helpers for common test operations</summary>
public static class TestHelpers
{
    /// <summary>Read body of request into an object</summary>
    public async static Task<T> ReadBody<T>(HttpResponseMessage message)
    {
        string rawBody = await message.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(rawBody))
            return default;

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        opts.Converters.Add(new JsonStringEnumConverter());

        return JsonSerializer.Deserialize<T>(rawBody, opts);
    }

    /// <summary>Convert object to HttpContent which can be POSTed</summary>
    public static HttpContent ToJsonBody(object obj)
    {
        string json = JsonSerializer.Serialize(obj);
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }
}