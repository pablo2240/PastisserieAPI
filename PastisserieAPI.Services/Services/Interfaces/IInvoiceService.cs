using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Services.Services.Interfaces
{
    public interface IInvoiceService
    {
        byte[] GenerateInvoicePdf(Pedido pedido, User usuario);
    }
}
