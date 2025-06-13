using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace UserManagementAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            string? fromEmail = _config["SmtpSettings:User"];
            string? smtpServer = _config["SmtpSettings:host"];
            string? smtpPortStr = _config["SmtpSettings:Port"];
            string? smtpUser = _config["SmtpSettings:User"];
            string? smtpPass = _config["SmtpSettings:Password"];

            // Validaciones para evitar null
            if (string.IsNullOrWhiteSpace(fromEmail)) throw new Exception("El correo remitente no está configurado.");
            if (string.IsNullOrWhiteSpace(smtpServer)) throw new Exception("El servidor SMTP no está configurado.");
            if (!int.TryParse(smtpPortStr, out int smtpPort)) throw new Exception("El puerto SMTP no es válido.");
            if (string.IsNullOrWhiteSpace(smtpUser)) throw new Exception("El usuario SMTP no está configurado.");
            if (string.IsNullOrWhiteSpace(smtpPass)) throw new Exception("La contraseña SMTP no está configurada.");

            // Preparar el correo
            var mail = new MailMessage(fromEmail, toEmail, subject, body);

            var smtp = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true // Puedes parametrizar esto también si quieres
            };

            await smtp.SendMailAsync(mail);
        }
    }
}
