using System.Threading.Tasks;

namespace HealthConnect.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string message);
        Task<bool> SendOtpAsync(string toEmail, string otp);

    }
}
