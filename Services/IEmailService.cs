using System.Threading.Tasks;

namespace TicTacToe.Services
{
    public interface IEmailService
    {
        Task SendEmail(string emailtTo, string subject, string message);
    }
}