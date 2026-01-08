using System.Text.Json;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using StoreScrapper.Models;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using StoreScrapper.Models.AdapterModels;

namespace StoreScrapper.Adapters;

public class ZaraAdapter : IStoreAdapter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly List<int> _alreadySentNotifications;
    private readonly TwilioOptions _twilioOptions;
    private readonly MailgunOptions _mailgunOptions;

    public ZaraAdapter(IHttpClientFactory httpClientFactory, List<int> alreadySentNotifications, IOptions<TwilioOptions> twilioOptions, IOptions<MailgunOptions> mailgunOptions)
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
        var model = JsonSerializer.Deserialize<ZaraModel>(webPageString)!;

        var available = model.skusAvailability
            .Where(x => x.availability == "in_stock" )
            .Where(x => !_alreadySentNotifications.Contains(x.sku))
            .FirstOrDefault();

        if (available != null)
        {
            var task1 = NotifyMail(productPageUrl, available.sku.ToString());
            var task2 = NotifyWhatsApp(productPageUrl, available.sku.ToString());

            await Task.WhenAll(task1, task2);
            _alreadySentNotifications.Add(available.sku);
        }

        return true;
    }

    private async Task NotifyMail(string link, string skuAvailable)
    {
        RestClient client = new RestClient(new Uri(_mailgunOptions.BaseUrl));
        client.Authenticator = new HttpBasicAuthenticator("api", _mailgunOptions.ApiKey);

        RestRequest request = new RestRequest();
        request.AddParameter("domain", _mailgunOptions.Domain, ParameterType.UrlSegment);
        request.Resource = "{domain}/messages";
        request.AddParameter("from", $"Excited User <mailgun@{_mailgunOptions.Domain}>");
        request.AddParameter("to", "jure.perak@hotmail.com");
        request.AddParameter("text", $"Dostupan artikl {link}\nsku: {skuAvailable}");
        request.AddParameter("subject", "Hurry!!!");
        request.Method = Method.Post;
        await client.ExecuteAsync(request);
    }

    private async Task NotifyWhatsApp(string link, string skuAvailable)
    {
        var messageOptions = new CreateMessageOptions(new PhoneNumber(_twilioOptions.SendToNumber));
        messageOptions.From = new PhoneNumber(_twilioOptions.SendFromNumber);
        messageOptions.Body = $"*Hurry!!!*\n{link}\n\n_{skuAvailable}_";

        var message = await MessageResource.CreateAsync(messageOptions);
    }
}
