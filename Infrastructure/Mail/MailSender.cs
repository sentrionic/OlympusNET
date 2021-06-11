using Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Mail
{
    public class MailSender : IMailSender
    {
        private SmtpClient Client { get; }
        
        public MailSender(IOptions<GmailSettings> config)
        {
            Client = new SmtpClient();
            Client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            Client.Authenticate(config.Value.User, config.Value.Password);
        }
        
        public void SendMail(string to, string html)
        {
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("OlympusBlog Team", "support@olympublog.com"));
            mailMessage.To.Add(new MailboxAddress("User", to));
            mailMessage.Subject = "Reset Password";
            mailMessage.Body = new TextPart("plain")
            {
                Text = html
            };
            
            Client.Send(mailMessage);
        }
    }
}