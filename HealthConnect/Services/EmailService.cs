using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using HealthConnect.Models;
using HealthConnect.Pages;

namespace HealthConnect.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string message)
        {
            string defaultCompanyName = "HealthConnect";
            string defaultGrantedMessage = "Thank you for using HealthConnect!";
            return await SendEmailAsync(toEmail, subject, message, defaultGrantedMessage, defaultCompanyName);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, string grantedMessage, string companyName)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                email.To.Add(new MailboxAddress(toEmail, toEmail));
                email.Subject = subject;

                var builder = new BodyBuilder
                {
                    TextBody = $"{body}\n\n{grantedMessage}\n\nRegards,\n{companyName}"
                };
                email.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, false);
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                    await client.SendAsync(email);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SendOtpAsync(string toEmail, string otp)
        {
            string subject = "Your OTP Code";
            string message = $"Your OTP code is: {otp}";
            return await SendEmailAsync(toEmail, subject, message);
        }
    }

}
