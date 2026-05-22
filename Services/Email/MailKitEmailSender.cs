using MailKit.Net.Smtp;
using MimeKit;

namespace FreelancerManagementSystem.Services.Email;

public class MailKitEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public MailKitEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlMessage)
    {
        var mailSettings = _configuration.GetSection("MailSettings");
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(mailSettings["From"] ?? "no-reply@example.com"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(mailSettings["Host"], int.Parse(mailSettings["Port"] ?? "2525"), bool.Parse(mailSettings["EnableSsl"] ?? "false"));
        await client.AuthenticateAsync(mailSettings["Username"], mailSettings["Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
