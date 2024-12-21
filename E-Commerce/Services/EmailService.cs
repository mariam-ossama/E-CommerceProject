using System.IO;
using System.Linq;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using E_Commerce.Models;
using System.Reflection.Metadata;

namespace E_Commerce.Services
{
    public class EmailService
    {
        private readonly EmailContentService _emailContentService;
        private readonly IConfiguration _configuration;

        public EmailService(EmailContentService emailContentService, IConfiguration configuration)
        {
            _emailContentService = emailContentService;
            _configuration = configuration;
        }

        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlMessage, byte[] attachmentData, string attachmentName)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };

            // Add attachment
            var attachment = bodyBuilder.Attachments.Add(attachmentName, attachmentData);
            attachment.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);

            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), useSsl: true);
                await smtpClient.AuthenticateAsync(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"]);
                await smtpClient.SendAsync(emailMessage);
                await smtpClient.DisconnectAsync(true);
            }
        }

        public async Task SendConfirmationEmailAsync(string email, string confirmationLink)
        {
            try
            {
                var emailContent = _emailContentService.GetConfirmationEmailContent(confirmationLink);
                await SendEmailAsync(email, "Email Confirmation", emailContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        public async Task SendResetPasswordEmail(string email, string resetPasswordLink)
        {
            try
            {
                var message = _emailContentService.GetResetEmailContent(resetPasswordLink);

                await SendEmailAsync(email, "Password Reset Request", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), useSsl: true);
                await smtpClient.AuthenticateAsync(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"]);
                await smtpClient.SendAsync(emailMessage);
                await smtpClient.DisconnectAsync(true);
            }
        }
    }
}
