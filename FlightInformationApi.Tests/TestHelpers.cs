using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightInformationApi.Tests;

public static class TestHelpers
{

    public async static Task<T> ReadBody<T>(System.Net.Http.HttpResponseMessage message)
    {
        string rawBody = await message.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(rawBody))
            return default;

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        opts.Converters.Add(new JsonStringEnumConverter());

        return JsonSerializer.Deserialize<T>(rawBody, opts);
    }


}