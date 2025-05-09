using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;

namespace LibraryAPI.Helpers
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
            
            var message = new MailMessage
            {
                From = new MailAddress("laibarystorer@gmail.com", "Library Store Notification"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            message.To.Add(email);

            using (var smtpClient = new SmtpClient(smtpSettings.Host, smtpSettings.Port))
            {
                smtpClient.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);
                smtpClient.EnableSsl = true;

                await smtpClient.SendMailAsync(message);
            }
        }
    }
}
