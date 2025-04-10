using System.Threading.Tasks;

namespace AllHoursCafe.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }
}
