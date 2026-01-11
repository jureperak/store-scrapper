using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using StoreScrapper.Data;
using StoreScrapper.Models;
using StoreScrapper.Models.Entities;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace StoreScrapper.Services;

public interface INotificationService
{
    Task<string> SendMailAsync(int productId, List<ProductSku> skuAvailable);
    
    Task<string> SendWhatsAppAsync(int productId, List<ProductSku> skuAvailable);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _appDbContext;
    private readonly TwilioOptions _twilioOptions;
    private readonly MailgunOptions _mailgunOptions;

    public NotificationService(IOptions<TwilioOptions> twilioOptions, IOptions<MailgunOptions> mailgunOptions, AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
        _twilioOptions = twilioOptions.Value;
        _mailgunOptions = mailgunOptions.Value;
    }

    public async Task<string> SendMailAsync(int productId, List<ProductSku> skusAvailable)
    {
        var product = await _appDbContext.Products
            .Where(x => x.Id == productId)
            .FirstAsync();
        
        var options = new RestClientOptions(_mailgunOptions.BaseUrl)
        {
            Authenticator = new HttpBasicAuthenticator("api", _mailgunOptions.ApiKey)
        };
        RestClient client = new RestClient(options);

        var request = new RestRequest($"/v3/{_mailgunOptions.Domain}/messages", Method.Post);
        request.AddParameter("from", $"Excited User <postmaster@{_mailgunOptions.Domain}>");

        var recipients = _mailgunOptions.Recipients.Split(';');
        foreach (var recipient in recipients)
        {
            request.AddParameter("to", recipient);
        }

        var skuOrSkus = skusAvailable.Count > 1 ? "skus" : "sku";
        var skuAvailable = string.Join("\n", skusAvailable
            .Select(x => $"{x.Name}: {x.Sku} => We will disable sending for this SKU until you reactivate at {x.ProductSkuReActivations.Single().ReEnableUrl}. You can reactivate in 30minutes. ({DateTime.UtcNow.AddMinutes(30):u})"));

        var body = $"Dostupno:\n{product.ProductPageUrl}\n\n{skuOrSkus}:\n{skuAvailable}";
        request.AddParameter("text", body);
        request.AddParameter("subject", $"Hurry!!! {product.Id}");
        var response = await client.ExecuteAsync(request);

        return body;
    }

    public async Task<string> SendWhatsAppAsync(int productId, List<ProductSku> skusAvailable)
    {
        var product = await _appDbContext.Products
            .Where(x => x.Id == productId)
            .FirstAsync();
        
        var messageOptions = new CreateMessageOptions(new PhoneNumber(_twilioOptions.SendToNumber));
        messageOptions.From = new PhoneNumber(_twilioOptions.SendFromNumber);
        
        var skuOrSkus = skusAvailable.Count > 1 ? "skus" : "sku";
        var skuAvailable = string.Join("\n", skusAvailable
            .Select(x => $"{x.Name}: _{x.Sku}_ => We will disable sending for this SKU until you reactivate at {x.ProductSkuReActivations.Single().ReEnableUrl}. You can reactivate in 30minutes. ({DateTime.UtcNow.AddMinutes(30):u})"));
        
        messageOptions.Body = $"*Hurry!!!*\n\nDostupno:\n{product.ProductPageUrl}\n\n{skuOrSkus}:\n{skuAvailable}";

        var message = await MessageResource.CreateAsync(messageOptions);
        
        return message.Body;
    }
}
