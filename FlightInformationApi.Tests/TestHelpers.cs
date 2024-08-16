using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightInformationApi.Tests;

public static class TestHelpers
{

    public async static Task<T> ReadBody<T>(ttpResponseMessage message)
    {
        string rawBody = await message.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(rawBody))
            return default;

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        opts.Converters.Add(new JsonStringEnumConverter());

        return JsonSerializer.Deserialize<T>(rawBody, opts);
    }

    public static HttpContent ToJsonBody(object obj)
    {
        string json = JsonSerializer.Serialize(obj);
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }
}