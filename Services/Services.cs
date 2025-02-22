namespace MyCoreWebApiCityInfo.Services;

public interface IMail
{
    void Send(string subject, string body);
}

public class LocalMail : IMail
{
    const string _mailTo = "admin@mycompany.com";
    const string _mailFrom = "noreply@mycompany.com";

    public void Send(string subject, string body)
    {
        string message = $"Mail from {_mailFrom} to {_mailTo} with {nameof(LocalMail)}{Environment.NewLine}";
        message += "Subject: " + subject + Environment.NewLine;
        message += "Message: " + body;

        Console.WriteLine(message);
    }
}

public class CloudMail : IMail
{
    const string _mailTo = "admin@mycompany.com";
    const string _mailFrom = "noreply@mycompany.com";

    public void Send(string subject, string body)
    {
        string message = $"Mail from {_mailFrom} to {_mailTo} with {nameof(CloudMail)}{Environment.NewLine}";
        message += "Subject: " + subject + Environment.NewLine;
        message += "Message: " + body;

        Console.WriteLine(message);
    }
}