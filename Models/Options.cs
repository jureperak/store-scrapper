namespace StoreScrapper.Models;

public class AppOptions
{
    public const string SectionName = "App";

    public string BaseUrl { get; set; } = string.Empty;
}

public class MailgunOptions
{
    public const string SectionName = "Mailgun";

    public string ApiKey { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Recipients { get; set; } = string.Empty;
}

public class TwilioOptions
{
    public const string SectionName = "Twilio";

    public string AccountSid { get; set; } = string.Empty;
    public string AccountToken { get; set; } = string.Empty;
    public string SendFromNumber { get; set; } = string.Empty;
    public string SendToNumber { get; set; } = string.Empty;
}