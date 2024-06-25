using AuthenticatedWebAPI.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AuthenticatedWebAPI.Service
{
    public class EmailService : IEmailService
    {
        private const string templatePath = @"EmailTemplate/{0}.html";
        private readonly SMTPConfigModel _smtpConfigModel;

        public EmailService(IOptions<SMTPConfigModel> smtpConfig)
        {
            _smtpConfigModel = smtpConfig.Value;
        }

        public async Task SendTestEmail(UserEmailOptions userEmailOptions)
        {
            userEmailOptions.Subject = "This is test email subject from Web App";
            userEmailOptions.Body = GetEmailBody("TestEmail");
            await SendEmail(userEmailOptions).ConfigureAwait(false);
        }

        private async Task SendEmail(UserEmailOptions userEmailOptions)
        {
            MailMessage mail = new MailMessage()
            {
                Subject = userEmailOptions.Subject,
                Body = userEmailOptions.Body,
                From = new MailAddress(_smtpConfigModel.SenderAddress, _smtpConfigModel.SenderDisplayName),
                IsBodyHtml = _smtpConfigModel.IsBodyHtml
            };
            foreach (var toEmail in userEmailOptions.ToEmails)
            {
                mail.To.Add(toEmail);
            }

            var client = new SmtpClient(_smtpConfigModel.Host, _smtpConfigModel.Port)
            {
                Credentials = new NetworkCredential(_smtpConfigModel.UserName, _smtpConfigModel.Password),
                EnableSsl = true,
            };
            await client.SendMailAsync(mail);
        }

        /*private async Task SendEmail(UserEmailOptions userEmailOptions)
        {
            MailMessage mail = new MailMessage()
            {
                Subject = userEmailOptions.Subject,
                Body = userEmailOptions.Body,
                From = new MailAddress(_smtpConfigModel.SenderAddress, _smtpConfigModel.SenderDisplayName),
                IsBodyHtml = _smtpConfigModel.IsBodyHtml
                
            };

            foreach (var toEmail in userEmailOptions.ToEmails)
            {
                mail.To.Add(toEmail);
            }
            NetworkCredential networkCredential = new NetworkCredential(_smtpConfigModel.UserName, _smtpConfigModel.Password);
            SmtpClient smtpClient = new SmtpClient()
            {
                Host = _smtpConfigModel.Host,
                Port = _smtpConfigModel.Port,
                EnableSsl = _smtpConfigModel.EnableSSL,
                UseDefaultCredentials = _smtpConfigModel.UseDefaultCredentials,
                Credentials = networkCredential
            };
            mail.BodyEncoding = Encoding.Default;
            await smtpClient.SendMailAsync(mail);
        }
*/
        private string GetEmailBody(string templateName)
        {
            var body = File.ReadAllText(string.Format(templatePath, templateName));
            return body;
        }

    }
}
