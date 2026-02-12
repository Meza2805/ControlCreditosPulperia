using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Net.Mail;
using SistemaPulperia.Models;

namespace SistemaPulperia.Services
{
    public class EmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IConfiguration _config;
        public EmailSender(IConfiguration config) => _config = config;

        // 1. Método para Enlaces de Confirmación de Cuenta
        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            string subject = "Confirma tu cuenta - Pulpería Doña Reyna";
            string message = $@"
                <h2 style='color: #d4af37;'>¡Bienvenido a la Pulpería Doña Reyna!</h2>
                <p>Para activar tu acceso privado, por favor confirma tu correo haciendo clic en el siguiente enlace:</p>
                <a href='{confirmationLink}' style='background: #0f172a; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Confirmar Cuenta</a>
                <p>Si no solicitaste este acceso, puedes ignorar este mensaje.</p>";

            await SendEmailAsync(email, subject, message);
        }

        // 2. Método para Enlaces de Restablecimiento de Contraseña
        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            string subject = "Restablecer Contraseña - Shekinah Services";
            string message = $@"
                <h2>Solicitud de cambio de contraseña</h2>
                <p>Hemos recibido una solicitud para restablecer tu contraseña. Haz clic en el botón de abajo:</p>
                <a href='{resetLink}' style='background: #d4af37; color: #0f172a; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Restablecer Clave</a>";

            await SendEmailAsync(email, subject, message);
        }

        // 3. Método para Códigos de Seguridad (2FA o Reset simple)
        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            string subject = "Código de Seguridad";
            string message = $"Tu código de seguridad para el sistema de la pulpería es: <b>{resetCode}</b>";

            await SendEmailAsync(email, subject, message);
        }

        // 4. LÓGICA BASE SMTP (TU CÓDIGO)
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpHost = _config["EmailSettings:Host"];
            var smtpPort = int.Parse(_config["EmailSettings:Port"] ?? "587");
            var senderEmail = _config["EmailSettings:Email"];
            var senderPass = _config["EmailSettings:Password"];

            var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(senderEmail, senderPass)
            };

            var mailMessage = new MailMessage(senderEmail!, email, subject, htmlMessage) 
            { 
                IsBodyHtml = true 
            };

            return client.SendMailAsync(mailMessage);
        }
    }
}