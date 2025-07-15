using DUANTOTNGHIEP.Models;
using System.Net;
using System.Net.Mail;

namespace DUANTOTNGHIEP.DTOS
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly SmtpSettings _smtpSettings;

        public EmailSender(IConfiguration config)
        {
            _config = config;
            _smtpSettings = _config.GetSection("SmtpSettings").Get<SmtpSettings>()!;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                var smtpClient = new SmtpClient(_smtpSettings.Host)
                {
                    Port = _smtpSettings.Port,
                    Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSSL,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 10000
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.User, "MCFoods"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                Console.WriteLine("❌ SMTP Exception: " + ex.Message);
                throw; // hoặc return Task.FromException(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Exception when sending email: " + ex.Message);
                throw;
            }
        }
    }

}
