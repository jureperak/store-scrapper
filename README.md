# Store Scrapper

.NET 10 web app that monitors Zara and Pull & Bear product availability and sends instant notifications via email and WhatsApp.

## Features

- Web UI for product management
- Automated Playwright scraping with size/SKU extraction
- Hangfire background jobs (checks every 5 seconds)
- PostgreSQL database with notification history
- Email (Mailgun) + WhatsApp (Twilio) notifications
- SKU-level monitoring with reactivation links
- Hangfire dashboard at `/hangfire`

## Origin Story

This project has quite the journey! It started during the great PS5 shortage when demand was so insane you couldn't buy one anywhere (remember those days?). The original goal was to monitor PS5 availability and snag one before the scalpers did.

Fast forward to today, and the project has evolved into something far more important: keeping my wife happy by monitoring her favorite clothing stores (Zara, Pull & Bear) for product availability. While these stores have their own notification systems, let's just say this scraper is significantly faster. When that perfect jacket drops back in stock, we know about it *immediately*.

From gaming consoles to fashion alerts - that's called pivoting! 😄

## Quick Start

**Prerequisites**: .NET 10 SDK, PostgreSQL, Mailgun account, Twilio/WhatsApp

1. **Install dependencies**
   ```bash
   dotnet restore
   pwsh bin/Debug/net10.0/playwright.ps1 install chromium
   ```

2. **Configure** `appsettings.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=your-db;Database=store_scrapper;Username=user;Password=pass",
       "HangfireConnection": "Host=your-db;Database=store_scrapper;Username=user;Password=pass"
     },
     "Mailgun": {
       "ApiKey": "your-key",
       "Domain": "your-domain.mailgun.org",
       "Recipients": "email1@example.com;email2@example.com"
     },
     "Twilio": {
       "AccountSid": "your-sid",
       "AuthToken": "your-token",
       "SendFromNumber": "whatsapp:+14155238886",
       "SendToNumber": "whatsapp:+1234567890"
     }
   }
   ```

3. **Run migrations & start**
   ```bash
   dotnet ef database update
   dotnet run
   ```

4. **Access**
   - App: http://localhost:5000
   - Hangfire: http://localhost:5000/hangfire

## Usage

**Add Product**: Navigate to http://localhost:5000 → "Scrape New Product" → Select store → Enter URL → Click "Scrape" → Select sizes → "Add Product for Scraping"

**Edit Product**: Click "Edit" → "Scrape" to refresh → New sizes shown in green with "NEW" badge → Adjust selection → "Save Changes"

**Manage**: Toggle enabled/disabled, archive products, view Hangfire dashboard for logs

## How It Works

**Database**: PostgreSQL with snake_case (products, product_skus, notification_history, job_execution_logs)

**Scraping**: Playwright extracts product data from `window.zara.viewPayload` and captures availability API URLs via network interception

**Jobs**: Hangfire creates one job per product (checks every 5 seconds), auto-retry on failure (3 attempts)

**Notifications**: When SKU becomes available → Email + WhatsApp sent → Recorded in DB → SKU temporarily disabled → Reactivation link included

## Project Structure

```
StoreScrapper/
├── Controllers/          # ProductController, ProductSkuController
├── Views/Product/        # Index, Create, Edit views
├── Services/             # StoreScrapingService, NotificationService, ProductPageScraperService
├── Adapters/             # ZaraAdapter, PullAndBearAdapter
├── Jobs/                 # StoreScrapingJob, HangfireJobManager
├── Models/Entities/      # Product, ProductSku, NotificationHistory
└── Data/                 # AppDbContext, Migrations
```

## API Endpoints

- `GET/POST /Product` - List, create, edit, delete products
- `POST /Product/ScrapeProductPage` - Scrape product data (JSON)
- `GET /api/productsku/{token}/reactivate` - Reactivate SKU

## Troubleshooting

- **Won't start**: Check PostgreSQL connection string, verify ports 5000/5001 available
- **Scraping fails**: Install Playwright browsers (`pwsh bin/Debug/net10.0/playwright.ps1 install chromium`)
- **No notifications**: Verify Mailgun/Twilio credentials, check Hangfire dashboard for errors
- **Timeout**: Increase timeout in `ProductPageScraperService.cs`, verify URL accessible

## Development

```bash
dotnet watch run  # Hot reload
```

View logs: Console output, Hangfire dashboard `/hangfire`, `job_execution_logs` table

---

**License**: Private project - All rights reserved
