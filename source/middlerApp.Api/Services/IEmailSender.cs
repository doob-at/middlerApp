using System.Threading.Tasks;

namespace middlerApp.Api.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
