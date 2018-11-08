using Infrastructure.System.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Manager.Email
{
    public abstract class IEmailService
    {
        protected IEmailConfiguration _emailSettings { get; set; }

        public async Task<bool> Send(string email, string subject, string message)
        {
            try
            {
                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.UsernameEmail, _emailSettings.UsernameDisplay),
                };

                mail.To.Add(new MailAddress(email));

                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;

                using (SmtpClient smtp = new SmtpClient(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort))
                {
                    smtp.Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
