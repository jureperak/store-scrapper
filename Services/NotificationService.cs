using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using StoreScrapper.Models;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace StoreScrapper.Services;

public class NotificationService
{
    private readonly TwilioOptions _twilioOptions;
    private readonly MailgunOptions _mailgunOptions;

    public NotificationService(IOptions<TwilioOptions> twilioOptions, IOptions<MailgunOptions> mailgunOptions)
    {
        _twilioOptions = twilioOptions.Value;
        _mailgunOptions = mailgunOptions.Value;
    }

    public async Task SendMailAsync(string link, string skuAvailable)
    {
        RestClient client = new RestClient(new Uri(_mailgunOptions.BaseUrl));
        client.Authenticator = new HttpBasicAuthenticator("api", _mailgunOptions.ApiKey);

        var recipients = _mailgunOptions.Recipients.Split(';');

        RestRequest request = new RestRequest();
        request.AddParameter("domain", _mailgunOptions.Domain, ParameterType.UrlSegment);
        request.Resource = "{domain}/messages";
        request.AddParameter("from", $"Excited User <mailgun@{_mailgunOptions.Domain}>");

        foreach (var recipient in recipients)
        {
            request.AddParameter("to", recipient);
        }

        request.AddParameter("text", $"Dostupan artikl {link}\nsku: {skuAvailable}");
        request.AddParameter("subject", "Hurry!!!");
        request.Method = Method.Post;
        await client.ExecuteAsync(request);
    }

    public async Task SendWhatsAppAsync(string link, string skuAvailable)
    {
        var messageOptions = new CreateMessageOptions(new PhoneNumber(_twilioOptions.SendToNumber));
        messageOptions.From = new PhoneNumber(_twilioOptions.SendFromNumber);
        messageOptions.Body = $"*Hurry!!!*\n{link}\n\n_{skuAvailable}_";

        var message = await MessageResource.CreateAsync(messageOptions);
    }
}
