using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Hangfire.Storage.SQLite;
using Microsoft.EntityFrameworkCore;
using StoreScrapper.Adapters;
using StoreScrapper.Data;
using StoreScrapper.Jobs;
using StoreScrapper.Models;
using StoreScrapper.Services;
using Twilio;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>((_, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention();
});

// Add Hangfire with PostgreSQL storage
builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UsePostgreSqlStorage(x => 
              x.UseNpgsqlConnection(builder.Configuration.GetConnectionString("HangfireConnection")),
              new PostgreSqlStorageOptions
              {
                  PrepareSchemaIfNecessary = true,
                  QueuePollInterval = TimeSpan.FromMilliseconds(500),
              }
            );
});

// Add Hangfire server
builder.Services.AddHangfireServer((options =>
{
    options.SchedulePollingInterval = TimeSpan.FromMilliseconds(500);
}));

// Configure options
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
builder.Services.Configure<MailgunOptions>(builder.Configuration.GetSection(MailgunOptions.SectionName));
builder.Services.Configure<TwilioOptions>(builder.Configuration.GetSection(TwilioOptions.SectionName));

// Register HttpClient
builder.Services.AddHttpClient();

// Register services
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddSingleton<HangfireJobManager>();
builder.Services.AddTransient<IStoreScrapingService, StoreScrapingService>();
builder.Services.AddTransient<IProductPageScraperService, ProductPageScraperService>();

// Register adapters as scoped
builder.Services.AddTransient<IZaraAdapter, ZaraAdapter>();
builder.Services.AddTransient<IPullAndBearAdapter, PullAndBearAdapter>();

// Register Hangfire job
builder.Services.AddScoped<StoreScrapingJob>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Configure Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // In production, add authentication
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Map default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");

var twilioOptions = builder.Configuration.GetSection(TwilioOptions.SectionName).Get<TwilioOptions>();
TwilioClient.Init(twilioOptions!.AccountSid, twilioOptions!.AccountToken);

app.Run();

// Simple authorization filter for Hangfire dashboard
// In production, implement proper authentication
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Allow all in development
        // In production, check user authentication/authorization
        return true;
    }
}
