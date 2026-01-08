using System.Text.Json;
using Microsoft.Extensions.Options;
using StoreScrapper.Models;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using StoreScrapper.Models.AdapterModels;

namespace StoreScrapper.Adapters;

public class PullAndBearAdapter : IStoreAdapter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly List<int> _alreadySentNotifications;
    private readonly TwilioOptions _twilioOptions;
    private readonly MailgunOptions _mailgunOptions;

    public PullAndBearAdapter(IHttpClientFactory httpClientFactory, List<int> alreadySentNotifications, IOptions<TwilioOptions> twilioOptions, IOptions<MailgunOptions> mailgunOptions)
    {
        _httpClientFactory = httpClientFactory;
        _alreadySentNotifications = alreadySentNotifications;
        _twilioOptions = twilioOptions.Value;
        _mailgunOptions = mailgunOptions.Value;
    }

    public async Task<bool> FetchAndProcessAsync(string availabilityUrl, string productPageUrl)
    {
        var httpClient = _httpClientFactory.CreateClient();
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, availabilityUrl);
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            return false;
        }

        var webPageString = await httpResponseMessage.Content.ReadAsStringAsync();
        var model = JsonSerializer.Deserialize<PullAndBearModel>(webPageString)!;

        var available = model.stocks
            .SelectMany(x => x.stocks)
            .Where(x => x.availability == "in_stock" )
            .Where(x => !_alreadySentNotifications.Contains(x.id))
            .FirstOrDefault();

        if (available != null)
        {
            await NotifyWhatsApp(productPageUrl, available.id.ToString());
            _alreadySentNotifications.Add(available.id);
        }

        return true;
    }

    private async Task NotifyWhatsApp(string link, string skuAvailable)
    {
        var messageOptions = new CreateMessageOptions(new PhoneNumber(_twilioOptions.SendToNumber));
        messageOptions.From = new PhoneNumber(_twilioOptions.SendFromNumber);
        messageOptions.Body = $"*Hurry!!!*\n{link}\n\n_{skuAvailable}_";

        var message = await MessageResource.CreateAsync(messageOptions);
    }
}
