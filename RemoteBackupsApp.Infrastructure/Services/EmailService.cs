using Microsoft.Extensions.Options;
using RemoteBackupsApp.Domain.ViewModels.Configs;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;

namespace RemoteBackupsApp.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private SmtpClient _smtp;
        private MailMessage mail;

        private string _hostsSmpt;
        private bool _enableSsl;
        private int _port;
        private string _senderEmail;
        private string _senderEmailPassword;
        private string _senderName;

        public EmailService(IOptions<SmtpSettings> emailParams)
        {
            _hostsSmpt = emailParams.Value.hostsSmpt;
            _enableSsl = emailParams.Value.enableSsl;
            _port = emailParams.Value.port;
            _senderEmail = emailParams.Value.senderEmail;
            _senderEmailPassword = emailParams.Value.senderEmailPassword;
            _senderName = emailParams.Value.senderName;
        }

        public async Task Send(string to, string subject, string body)
        {
            mail = new MailMessage();
            mail.From = new MailAddress(_senderEmail, _senderName);
            mail.To.Add(new MailAddress(to));
            mail.Subject = subject;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.Body = body;

            _smtp = new SmtpClient()
            {
                Host = _hostsSmpt,
                EnableSsl = _enableSsl,
                Port = _port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_senderEmail, _senderEmailPassword)
            };

            _smtp.SendCompleted += OnSendCompleted;

            await _smtp.SendMailAsync(mail);
        }

        private void OnSendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _smtp.Dispose();
            mail.Dispose();
        }
    }
}
