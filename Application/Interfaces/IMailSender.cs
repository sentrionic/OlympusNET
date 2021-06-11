namespace Application.Interfaces
{
    public interface IMailSender
    {
        void SendMail(string to, string html);
    }
}