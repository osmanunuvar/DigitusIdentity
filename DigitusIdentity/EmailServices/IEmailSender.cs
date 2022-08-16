using System.Threading.Tasks;

namespace DigitusIdentity.EmailServices
{
    public interface IEmailSender
    {

        Task SendEmailAsync(string email, string subject, string htmlmessage);
    }
}
