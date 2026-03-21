namespace PastisserieAPI.Services.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendWelcomeEmailAsync(string to, string userName);
        Task SendOrderConfirmationEmailAsync(string to, string userName, int orderId, decimal total, byte[]? invoicePdf = null);
        Task SendOrderStatusUpdateEmailAsync(string to, string userName, int orderId, string newStatus);
        Task SendPasswordResetEmailAsync(string to, string resetLink);
        Task SendInvoiceEmailAsync(string to, string userName, int orderId, byte[] pdfBytes);
        Task SendRepartidorAssignmentEmailAsync(string to, string repartidorName, int orderId, string clienteNombre, string direccion);
    }
}
