using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.UI.Services;
using Mailjet.Client;
using Newtonsoft.Json.Linq;
using Mailjet.Client.Resources;

namespace WebDoDienTu.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;
        public AuthMessageSenderOptions Options { get; }

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor, ILogger<EmailSender> logger)
        {
            Options = optionsAccessor.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (string.IsNullOrEmpty(Options.MailjetApiKey) || string.IsNullOrEmpty(Options.MailjetApiSecret))
            {
                throw new Exception("Null MailjetApiKey or MailjetApiSecret");
            }
            await Execute(Options.MailjetApiKey, Options.MailjetApiSecret, subject, message, toEmail);
        }

        public async Task Execute(string apiKey, string apiSecret, string subject, string message, string toEmail)
        {
            var client = new MailjetClient(apiKey, apiSecret);

            var request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
            .Property(Send.FromEmail, "buiducnhan.it@gmail.com")
            .Property(Send.FromName, "Bùi Đức Nhân")
            .Property(Send.Subject, subject)
            .Property(Send.HtmlPart, message)
            .Property(Send.Recipients, new JArray {
            new JObject {
                { "Email", toEmail }
            }
            });

            var response = await client.PostAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Email to {toEmail} queued successfully!");
            }
            else
            {
                _logger.LogError($"Failure Email to {toEmail}: {response.GetErrorMessage()}");
            }
        }
    }
}
