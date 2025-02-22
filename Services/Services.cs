using Microsoft.Extensions.Configuration;

namespace MyCoreWebApiCityInfo.Services;

public interface IMail
{
    void Send(string subject, string body);
}

public class LocalMail(IConfiguration config) : IMail
{
    readonly string _mailTo = config["mailSettings:mailToAddress"] ?? throw new ArgumentNullException(nameof(config));
    readonly string _mailFrom = config["mailSettings:mailFromAddress"] ?? throw new ArgumentNullException(nameof(config));

    public void Send(string subject, string body)
    {
        string message = $"Mail from {_mailFrom} to {_mailTo} with {nameof(LocalMail)}{Environment.NewLine}";
        message += "Subject: " + subject + Environment.NewLine;
        message += "Message: " + body;

        Console.WriteLine(message);
    }
}

public class CloudMail(IConfiguration config) : IMail
{
    readonly string _mailTo = config["mailSettings:mailToAddress"] ?? throw new ArgumentNullException(nameof(config));
    readonly string _mailFrom = config["mailSettings:mailFromAddress"] ?? throw new ArgumentNullException(nameof(config));

    public void Send(string subject, string body)
    {
        string message = $"Mail from {_mailFrom} to {_mailTo} with {nameof(CloudMail)}{Environment.NewLine}";
        message += "Subject: " + subject + Environment.NewLine;
        message += "Message: " + body;

        Console.WriteLine(message);
    }
}