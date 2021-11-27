using System.Threading.Tasks;

namespace middlerApp.Api.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
