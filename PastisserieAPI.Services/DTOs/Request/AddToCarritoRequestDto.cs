namespace PastisserieAPI.Services.DTOs.Request
{
    public class AddToCarritoRequestDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; } = 1;
    }
}