using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using PastisserieAPI.Services.Services.Interfaces;

namespace PastisserieAPI.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var host = _config["Smtp:Host"];
            var portStr = _config["Smtp:Port"];
            var user = _config["Smtp:User"];
            var password = _config["Smtp:Password"];
            var fromName = _config["Smtp:FromName"] ?? "Pâtisserie Deluxe";

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("EmailService: SMTP settings are missing. Email not sent.");
                return;
            }

            int port = int.TryParse(portStr, out var p) ? p : 587;

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(fromName, user));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            smtp.Timeout = 15000; // Un poco más de tiempo por si acaso
            try
            {
                // Usar StartTls para el puerto 587 (más estándar para Gmail)
                var socketOptions = port == 587 ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
                
                _logger.LogInformation($"EmailService: Connecting to {host}:{port} with {socketOptions}...");
                await smtp.ConnectAsync(host, port, socketOptions);
                
                _logger.LogInformation($"EmailService: Authenticating {user}...");
                await smtp.AuthenticateAsync(user, password);
                
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                _logger.LogInformation($"EmailService: Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EmailService ERROR: Failed to send email to {to}. Host: {host}, Port: {port}, User: {user}");
                throw; // Rethrow to allow controller to handle the failure
            }
        }

        public async Task SendWelcomeEmailAsync(string to, string userName)
        {
            string body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px;'>
                    <h2 style='color: #7D2121;'>¡Bienvenido a Pâtisserie Deluxe, {userName}!</h2>
                    <p>Gracias por unirte a nuestra comunidad de amantes de la pastelería artesanal.</p>
                    <p>Ahora puedes realizar tus pedidos y disfrutar de lo mejor en repostería.</p>
                    <div style='text-align: center; margin-top: 30px;'>
                        <a href='https://pastisserie-deluxe.com' style='background-color: #7D2121; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Explorar Productos</a>
                    </div>
                </div>";
            await SendEmailAsync(to, "Bienvenido a Pâtisserie Deluxe", body);
        }

        public async Task SendOrderConfirmationEmailAsync(string to, string userName, int orderId, decimal total, byte[]? invoicePdf = null)
        {
            var host = _config["Smtp:Host"];
            var portStr = _config["Smtp:Port"];
            var user = _config["Smtp:User"];
            var password = _config["Smtp:Password"];
            var fromName = _config["Smtp:FromName"] ?? "Pâtisserie Deluxe";

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("EmailService: SMTP settings missing for Order Confirmation.");
                return;
            }

            int port = int.TryParse(portStr, out var p) ? p : 587;

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(fromName, user));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = $"Confirmación de Pedido #{orderId}";

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px;'>
                    <h2 style='color: #7D2121;'>¡Pedido Confirmado!</h2>
                    <p>Hola {userName}, hemos recibido tu pedido <strong>#{orderId}</strong>.</p>
                    <p>Total a pagar: <strong>${total:N0} COP</strong></p>
                    <p>Estamos preparando tus delicias con mucho amor. Te notificaremos cuando el estado cambie.</p>
                    <hr style='border: 0; border-top: 1px solid #eee;' />
                    <p style='font-size: 12px; color: #777;'>Si tienes dudas, contáctanos a nuestro WhatsApp.</p>
                </div>"
            };

            if (invoicePdf != null)
            {
                builder.Attachments.Add($"Factura_Pedido_{orderId}.pdf", invoicePdf, ContentType.Parse("application/pdf"));
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Timeout = 15000;
            try
            {
                var socketOptions = port == 587 ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
                await smtp.ConnectAsync(host, port, socketOptions);
                await smtp.AuthenticateAsync(user, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EmailService ERROR: Failed to send order confirmation to {to}");
            }
        }

        public async Task SendOrderStatusUpdateEmailAsync(string to, string userName, int orderId, string newStatus)
        {
            string body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px;'>
                    <h2 style='color: #7D2121;'>Actualización de tu Pedido</h2>
                    <p>Hola {userName}, el estado de tu pedido <strong>#{orderId}</strong> ha cambiado a:</p>
                    <div style='background-color: #f9f9f9; padding: 15px; text-align: center; font-size: 18px; font-weight: bold; color: #7D2121; border-radius: 5px; margin: 20px 0;'>
                        {newStatus}
                    </div>
                    <p>¡Pronto tendrás tus productos contigo!</p>
                </div>";
            await SendEmailAsync(to, $"Estado Actualizado - Pedido #{orderId}", body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetLink)
        {
            string body = $@"
                <div style='font-family: ""Playfair Display"", serif; max-width: 600px; margin: auto; border: 1px solid #f0f0f0; padding: 40px; border-radius: 16px; background-color: #ffffff; box-shadow: 0 4px 12px rgba(0,0,0,0.05);'>
                    <div style='text-align: center; margin-bottom: 30px;'>
                        <h1 style='color: #7D2121; margin: 0; font-size: 28px;'>Pâtisserie Deluxe</h1>
                        <p style='color: #999; font-size: 12px; text-transform: uppercase; letter-spacing: 2px; margin-top: 5px;'>Excelencia en Repostería Artersanal</p>
                    </div>
                    
                    <h2 style='color: #333; text-align: center; font-size: 22px; margin-bottom: 20px;'>Recupera tu Acceso</h2>
                    
                    <p style='color: #666; font-size: 16px; line-height: 1.6; text-align: center;'>
                        Hemos recibido una solicitud para restablecer tu contraseña. Si no fuiste tú, puedes ignorar este mensaje con tranquilidad.
                    </p>
                    
                    <div style='text-align: center; margin: 40px 0;'>
                        <a href='{resetLink}' style='background-color: #7D2121; color: #ffffff; padding: 18px 36px; text-decoration: none; border-radius: 12px; font-weight: bold; font-size: 16px; display: inline-block; transition: all 0.3s ease; box-shadow: 0 4px 10px rgba(125, 33, 33, 0.3);'>
                            Restablecer Contraseña
                        </a>
                    </div>
                    
                    <p style='color: #999; font-size: 13px; text-align: center; margin-top: 30px;'>
                        Este enlace expirará pronto por razones de seguridad.
                    </p>
                    
                    <div style='border-top: 1px solid #eee; margin-top: 40px; padding-top: 20px; text-align: center; color: #bbb; font-size: 11px;'>
                        Patisserie Deluxe S.A.S • Av. Principal 123 • Bogotá, Colombia<br>
                        © {DateTime.Now.Year} Todos los derechos reservados
                    </div>
                </div>";
            await SendEmailAsync(to, "Restablecer Contraseña - Pâtisserie Deluxe", body);
        }

        public async Task SendInvoiceEmailAsync(string to, string userName, int orderId, byte[] pdfBytes)
        {
            var host = _config["Smtp:Host"];
            var portStr = _config["Smtp:Port"];
            var user = _config["Smtp:User"];
            var password = _config["Smtp:Password"];
            var fromName = _config["Smtp:FromName"] ?? "Pâtisserie Deluxe";

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("EmailService: SMTP settings missing for Invoice.");
                return;
            }

            int port = int.TryParse(portStr, out var p) ? p : 587;

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(fromName, user));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = $"Tu Factura - Pedido #{orderId}";

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; max-width: 600px; border: 1px solid #eee;'>
                    <h2 style='color: #7D2121;'>¡Gracias por tu compra, {userName}!</h2>
                    <p>Adjunto encontrarás la factura detallada de tu pedido <strong>#{orderId}</strong>.</p>
                    <p>Esperamos que disfrutes nuestras delicias.</p>
                </div>"
            };

            builder.Attachments.Add($"Factura_Pedido_{orderId}.pdf", pdfBytes, ContentType.Parse("application/pdf"));
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Timeout = 15000;
            try
            {
                var socketOptions = port == 587 ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
                await smtp.ConnectAsync(host, port, socketOptions);
                await smtp.AuthenticateAsync(user, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EmailService ERROR: Failed to send invoice to {to}");
            }
        }

        public async Task SendRepartidorAssignmentEmailAsync(string to, string repartidorName, int orderId, string clienteNombre, string direccion)
        {
            string body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px;'>
                    <h2 style='color: #7D2121;'>¡Nuevo Pedido Asignado!</h2>
                    <p>Hola <strong>{repartidorName}</strong>, se te ha asignado una nueva entrega.</p>
                    <div style='background-color: #f9f9f9; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <p><strong>Pedido:</strong> #{orderId}</p>
                        <p><strong>Cliente:</strong> {clienteNombre}</p>
                        <p><strong>Dirección:</strong> {direccion}</p>
                    </div>
                    <p>Por favor, revisa tu panel de repartidor para ver los detalles y marcar el inicio de la entrega.</p>
                </div>";
            await SendEmailAsync(to, $"Nueva Asignación - Pedido #{orderId}", body);
        }
    }
}
