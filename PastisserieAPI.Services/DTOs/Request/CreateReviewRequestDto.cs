namespace PastisserieAPI.Services.DTOs.Request
{
    public class CreateReviewRequestDto
    {
        public int ProductoId { get; set; }
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
    }
}