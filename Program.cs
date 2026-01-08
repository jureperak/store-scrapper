using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StoreScrapper.Adapters;
using StoreScrapper.Models;
using StoreScrapper.Models.AdapterModels;
using StoreScrapper.Services;
using Twilio;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Create notification lists outside DI
var alreadySentNotificationZara = new List<int>();
var alreadySentNotificationPullAndBear = new List<int>();

var serviceProvider = new ServiceCollection()
    .AddHttpClient()
    .Configure<MailgunOptions>(configuration.GetSection(MailgunOptions.SectionName))
    .Configure<TwilioOptions>(configuration.GetSection(TwilioOptions.SectionName))
    // Register concrete adapter types as singletons
    .AddSingleton<ZaraAdapter>(sp => new ZaraAdapter(
        sp.GetRequiredService<IHttpClientFactory>(),
        alreadySentNotificationZara,
        sp.GetRequiredService<IOptions<TwilioOptions>>(),
        sp.GetRequiredService<IOptions<MailgunOptions>>()
    ))
    .AddSingleton<PullAndBearAdapter>(sp => new PullAndBearAdapter(
        sp.GetRequiredService<IHttpClientFactory>(),
        alreadySentNotificationPullAndBear,
        sp.GetRequiredService<IOptions<TwilioOptions>>(),
        sp.GetRequiredService<IOptions<MailgunOptions>>()
    ))
    .AddSingleton<StoreScrapperService>()
    .BuildServiceProvider();

// Initialize Twilio with options
var twilioConfig = serviceProvider.GetRequiredService<IOptions<TwilioOptions>>().Value;
TwilioClient.Init(twilioConfig.AccountSid, twilioConfig.AuthToken);

// Get scrapper service from DI container
var scrapperService = serviceProvider.GetRequiredService<StoreScrapperService>();
await scrapperService.RunAsync();

Console.WriteLine("Done");