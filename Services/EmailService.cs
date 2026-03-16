using System.Net;
using System.Net.Mail;
using FortunaCasino.Services.Interfaces;

namespace FortunaCasino.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var smtpServer = _config["Email:SmtpServer"];
            var smtpPort = int.Parse(_config["Email:SmtpPort"]);
            var smtpUser = _config["Email:SmtpUser"];
            var smtpPass = _config["Email:SmtpPass"];
            var fromEmail = _config["Email:From"];

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true,
                Timeout = 10000
            };

            var message = new MailMessage(fromEmail, to, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email küldési hiba: {ex.Message}");
            return false;
        }
    }
}