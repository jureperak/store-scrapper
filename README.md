# Store Scrapper

A .NET 9 console application that monitors product availability across multiple online stores (Zara, Pull & Bear) and sends notifications via email and WhatsApp when products become available.

## Features

- Monitor multiple stores simultaneously
- Filter products by specific SKU or exclude certain SKUs
- Email notifications via Mailgun
- WhatsApp notifications via Twilio
- Configuration-based store management
- Automatic retry with configurable intervals
- Prevents duplicate notifications

## Origin Story

This project has quite the journey! It started during the great PS5 shortage when demand was so insane you couldn't buy one anywhere (remember those days?). The original goal was to monitor PS5 availability and snag one before the scalpers did.

Fast forward to today, and the project has evolved into something far more important: keeping my wife happy by monitoring her favorite clothing stores (Zara, Pull & Bear) for product availability. While these stores have their own notification systems, let's just say this scraper is significantly faster. When that perfect jacket drops back in stock, we know about it *immediately*.

From gaming consoles to fashion alerts - that's called pivoting! 😄

## Prerequisites

- .NET 9.0 SDK
- Mailgun account and API key
- Twilio account with WhatsApp sandbox configured

## Setup

1. **Clone the repository**
   ```bash
   cd store-scrapper
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure application settings**

   Create or update `appsettings.json`:
   ```json
   {
     "Mailgun": {
       "ApiKey": "your-mailgun-api-key",
       "Domain": "your-domain.mailgun.org",
       "BaseUrl": "https://api.mailgun.net/v3",
       "Recipients": "email1@example.com;email2@example.com"
     },
     "Twilio": {
       "AccountSid": "your-twilio-account-sid",
       "AuthToken": "your-twilio-auth-token",
       "SendFromNumber": "whatsapp:+14155238886",
       "SendToNumber": "whatsapp:+1234567890"
     }
   }
   ```

4. **Configure stores to monitor**

   Create or update `stores.json`:
   ```json
   {
     "stores": [
       {
         "adapter": "Zara",
         "availabilityUrl": "https://www.zara.com/itxrest/1/catalog/store/11737/product/id/316571537/availability?ajax=true",
         "productPageUrl": "https://www.zara.com/hr/hr/product-page.html",
         "neededProduct": 12345,
         "productsToAvoid": [67890, 11111]
       }
     ]
   }
   ```

## Configuration

### appsettings.json

- **Mailgun.ApiKey**: Your Mailgun API key
- **Mailgun.Domain**: Your Mailgun domain
- **Mailgun.BaseUrl**: Mailgun API base URL (usually `https://api.mailgun.net/v3`)
- **Mailgun.Recipients**: Semicolon-separated list of email recipients
- **Twilio.AccountSid**: Your Twilio account SID
- **Twilio.AuthToken**: Your Twilio auth token
- **Twilio.SendFromNumber**: Twilio WhatsApp sender number (format: `whatsapp:+14155238886`)
- **Twilio.SendToNumber**: Your WhatsApp number (format: `whatsapp:+1234567890`)

### stores.json

Each store entry requires:
- **adapter**: Store adapter to use (`"Zara"` or `"PullAndBear"`)
- **availabilityUrl**: API endpoint to check product availability
- **productPageUrl**: Product page URL to include in notifications
- **neededProduct**: (Optional) Specific product SKU to monitor (null to monitor all)
- **productsToAvoid**: (Optional) Array of product SKUs to exclude from monitoring

## Usage

Run the application:
```bash
dotnet run
```

The application will:
1. Load all stores from `stores.json`
2. Check availability for each store
3. Send notifications when products become available
4. Wait 4 seconds between cycles
5. Continue monitoring until manually stopped (Ctrl+C)

## Adding New Stores

To monitor additional products, add entries to `stores.json`:

```json
{
  "stores": [
    {
      "adapter": "Zara",
      "availabilityUrl": "https://www.zara.com/itxrest/1/catalog/store/11737/product/id/316571537/availability?ajax=true",
      "productPageUrl": "https://www.zara.com/hr/hr/product-1.html",
      "neededProduct": null,
      "productsToAvoid": []
    },
    {
      "adapter": "PullAndBear",
      "availabilityUrl": "https://www.pullandbear.com/itxrest/2/catalog/store/25009/product/id/123456/stock",
      "productPageUrl": "https://www.pullandbear.com/hr/product-2.html",
      "neededProduct": 78901,
      "productsToAvoid": [11111, 22222]
    }
  ]
}
```

## Project Structure

```
StoreScrapper/
├── Adapters/
│   ├── IStoreAdapter.cs          # Interface for store adapters
│   ├── ZaraAdapter.cs             # Zara-specific scraping logic
│   └── PullAndBearAdapter.cs      # Pull & Bear scraping logic
├── Services/
│   ├── StoreScrapperService.cs    # Main scraping orchestration
│   └── NotificationService.cs     # Email and WhatsApp notifications
├── Models/
│   ├── Options.cs                 # Configuration option classes
│   ├── StoreConfiguration.cs      # Store configuration models
│   └── AdapterModels/             # Store-specific API response models
├── Program.cs                     # Application entry point
├── appsettings.json              # API credentials configuration
├── stores.json                   # Store monitoring configuration
└── StoreScrapper.csproj          # Project file
```

## How It Works

1. **Initialization**: Reads configuration from `appsettings.json` and `stores.json`
2. **Dependency Injection**: Sets up services and adapters with proper dependencies
3. **Monitoring Loop**:
   - Iterates through all configured stores
   - Selects appropriate adapter based on store type
   - Fetches availability data from store API
   - Filters products based on configuration
   - Sends notifications if new products are available
   - Tracks notified products to prevent duplicates
4. **Retry**: Waits 4 seconds and repeats the cycle

## Notifications

When a product becomes available:
- **Email**: Sent to all recipients listed in `Mailgun.Recipients`
- **WhatsApp**: Sent to the number specified in `Twilio.SendToNumber`
- Both notifications include the product page URL and SKU(s)

## Troubleshooting

- **No notifications received**: Verify API credentials in `appsettings.json`
- **Unknown adapter error**: Check that `adapter` field in `stores.json` is either `"Zara"` or `"PullAndBear"`
- **Build errors**: Ensure .NET 9 SDK is installed (`dotnet --version`)

## Notes

- The application will continue running until manually stopped
- Each adapter maintains its own list of notified products
- Notification lists reset when the application is restarted
- API rate limits may apply depending on store endpoints
