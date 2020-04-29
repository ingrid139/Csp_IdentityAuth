using IdentityAuth.Models;
using System.Threading.Tasks;

namespace IdentityAuth.Services
{
    public interface IEmailServices
    {
        Task<EmailResponse> SendEmailBySendGridAsync(string email, string assunto, string mensagem);
    }
}