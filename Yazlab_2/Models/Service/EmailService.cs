using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Yazlab_2.Models.Service
{
    public class EmailService
    {
        public async Task SendResetPasswordEmail(string toEmail, string resetLink)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587, 
                Credentials = new NetworkCredential("tugbik6@gmail.com", "kvni zwot sgfx gouj"), 
                EnableSsl = true 
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("your-email@gmail.com"), 
                Subject = "Şifre Sıfırlama Talebi",
                Body = $"Şifrenizi sıfırlamak için linke tıklayın: {resetLink}",
                IsBodyHtml = true 
            };

            mailMessage.To.Add(toEmail); 

            try
            {
                await smtpClient.SendMailAsync(mailMessage); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                throw;
            }
        }
    }

}

