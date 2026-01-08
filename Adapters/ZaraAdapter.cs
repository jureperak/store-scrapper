using System.Text.Json;
using StoreScrapper.Models;
using StoreScrapper.Services;

namespace StoreScrapper.Adapters;

public class ZaraAdapter : IStoreAdapter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly List<int> _alreadySentNotifications;
    private readonly NotificationService _notificationService;

    public ZaraAdapter(IHttpClientFactory httpClientFactory, List<int> alreadySentNotifications, NotificationService notificationService)
    {
        _httpClientFactory = httpClientFactory;
        _alreadySentNotifications = alreadySentNotifications;
        _notificationService = notificationService;
    }

    public async Task<bool> FetchAndProcessAsync(string availabilityUrl, string productPageUrl, int? neededProduct, List<int> productsToAvoid)
    {
        var httpClient = _httpClientFactory.CreateClient();
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, availabilityUrl);
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            return false;
        }

        var webPageString = await httpResponseMessage.Content.ReadAsStringAsync();
        var model = JsonSerializer.Deserialize<ZaraModel>(webPageString)!;

        var availableProducts = model.skusAvailability
            .Where(x => x.availability == "in_stock" )
            .Where(x => !_alreadySentNotifications.Contains(x.sku))
            .ToList();
        
        if (neededProduct.HasValue)
        {
            availableProducts = availableProducts
                .Where(x => x.sku == neededProduct)
                .ToList();
        }

        if (productsToAvoid.Any())
        {
            availableProducts = availableProducts
                .Where(x => !productsToAvoid.Contains(x.sku))
                .ToList();
        }

        if (availableProducts.Any())
        {
            var availableSkus = availableProducts
                .Select(x => x.sku)
                .ToList();

            var task1 = _notificationService.SendMailAsync(productPageUrl, string.Join(", ", availableSkus));
            var task2 = _notificationService.SendWhatsAppAsync(productPageUrl, string.Join(", ", availableSkus));

            await Task.WhenAll(task1, task2);

            _alreadySentNotifications.AddRange(availableSkus);
        }

        return true;
    }
}
