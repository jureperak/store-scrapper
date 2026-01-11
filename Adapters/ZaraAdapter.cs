using System.Text.Json;
using StoreScrapper.Models;

namespace StoreScrapper.Adapters;

public interface IZaraAdapter : IStoreAdapter;

public class ZaraAdapter : IZaraAdapter
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ZaraAdapter(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AdapterResult> FetchAndProcessAsync(string availabilityUrl, List<int> neededProductSkus)
    {
        var httpClient = _httpClientFactory.CreateClient();
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, availabilityUrl);
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            return AdapterResult.ErrorResult($"HTTP request failed with status code: {httpResponseMessage.StatusCode}");
        }

        var webPageString = await httpResponseMessage.Content.ReadAsStringAsync();

        ZaraModel? model;
        
        try
        {
            model = JsonSerializer.Deserialize<ZaraModel>(webPageString);
            if (model == null)
            {
                return AdapterResult.ErrorResult("Failed to deserialize JSON response");
            }
        }
        catch (JsonException ex)
        {
            return AdapterResult.ErrorResult($"JSON deserialization error: {ex.Message}");
        }

        // Filter available products
        var availableProducts = model.SkusAvailability
            .Where(x => x.Availability == "in_stock")
            .Where(x => neededProductSkus.Contains(x.Sku))
            .ToList();

        // Return the list of available SKUs
        var availableSkus = availableProducts
            .Select(x => x.Sku)
            .ToList();

        return AdapterResult.SuccessResult(availableSkus);
    }
}
