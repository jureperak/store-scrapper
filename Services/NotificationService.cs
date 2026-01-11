using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using StoreScrapper.Models;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace StoreScrapper.Services;

public interface INotificationService
{
    Task<string> SendMailAsync(string link, List<(int Sku, string Name)> skuAvailable);
    
    Task<string> SendWhatsAppAsync(string link, List<(int Sku, string Name)> skuAvailable);
}

public class NotificationService : INotificationService
{
    private readonly TwilioOptions _twilioOptions;
    private readonly MailgunOptions _mailgunOptions;

    public NotificationService(IOptions<TwilioOptions> twilioOptions, IOptions<MailgunOptions> mailgunOptions)
    {
        _twilioOptions = twilioOptions.Value;
        _mailgunOptions = mailgunOptions.Value;
    }

    public async Task<string> SendMailAsync(string link, List<(int Sku, string Name)> skusAvailable)
    {
        var options = new RestClientOptions(_mailgunOptions.BaseUrl)
        {
            Authenticator = new HttpBasicAuthenticator("api", _mailgunOptions.ApiKey)
        };
        RestClient client = new RestClient(options);

        var recipients = _mailgunOptions.Recipients.Split(';');

        RestRequest request = new RestRequest("{domain}/messages", Method.Post);
        request.AddParameter("domain", _mailgunOptions.Domain, ParameterType.UrlSegment);
        request.AddParameter("from", $"Excited User <mailgun@{_mailgunOptions.Domain}>");

        foreach (var recipient in recipients)
        {
            request.AddParameter("to", recipient);
        }

        var skuOrSkus = skusAvailable.Count > 1 ? "skus" : "sku";
        var skuAvailable = string.Join("\n", skusAvailable.Select(x => $"{x.Name}: {skuOrSkus}"));

        var body = $"Dostupno:\n{link}\n\n{skuOrSkus}\n:{skuAvailable}";
        request.AddParameter("text", body);
        request.AddParameter("subject", "Hurry!!!");
        await client.ExecuteAsync(request);

        return body;
    }

    public async Task<string> SendWhatsAppAsync(string link, List<(int Sku, string Name)> skusAvailable)
    {
        var messageOptions = new CreateMessageOptions(new PhoneNumber(_twilioOptions.SendToNumber));
        messageOptions.From = new PhoneNumber(_twilioOptions.SendFromNumber);
        
        var skuOrSkus = skusAvailable.Count > 1 ? "skus" : "sku";
        var skuAvailable = string.Join("\n", skusAvailable.Select(x => $"{x.Name}: _{skuOrSkus}_"));
        
        messageOptions.Body = $"*Hurry!!!*\n\nDostupno:\n{link}\n\n{skuOrSkus}:\n{skuAvailable}";

        var message = await MessageResource.CreateAsync(messageOptions);
        
        return message.Body;
    }
}
