namespace PastisserieAPI.Services.DTOs.Response
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
        public DateTime Fecha { get; set; }
        public bool Aprobada { get; set; }
    }
}