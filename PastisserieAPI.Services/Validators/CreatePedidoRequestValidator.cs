using FluentValidation;
using PastisserieAPI.Services.DTOs.Request;

namespace PastisserieAPI.Services.Validators
{
    /// <summary>
    /// Validador para la creación de pedidos (F5 - Checkout)
    /// </summary>
    public class CreatePedidoRequestValidator : AbstractValidator<CreatePedidoRequestDto>
    {
        public CreatePedidoRequestValidator()
        {
            // ========== VALIDACIÓN USUARIO (OBLIGATORIO) ==========
            RuleFor(x => x.UsuarioId)
                .GreaterThan(0)
                .WithMessage("El ID de usuario es inválido");

            // ========== VALIDACIÓN DIRECCIÓN DE ENVÍO (F5 - OBLIGATORIA) ==========
            RuleFor(x => x.DireccionEnvioId)
                .NotNull()
                .WithMessage("Debe seleccionar una dirección de envío")
                .GreaterThan(0)
                .WithMessage("La dirección de envío es inválida");

            // ========== VALIDACIÓN DE ITEMS (F5 - MÍNIMO 1 PRODUCTO) ==========
            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("El pedido debe tener al menos un producto")
                .Must(items => items != null && items.Count > 0)
                .WithMessage("El pedido debe tener al menos un producto")
                .Must(items => items.Count <= 30)
                .WithMessage("No puede agregar más de 30 productos diferentes al pedido");

            // ========== VALIDACIÓN INDIVIDUAL DE CADA ITEM ==========
            RuleForEach(x => x.Items)
                .SetValidator(new PedidoItemRequestValidator());

            // ========== VALIDACIÓN NOTAS CLIENTE (OPCIONAL) ==========
            RuleFor(x => x.NotasCliente)
                .MaximumLength(1000)
                .WithMessage("Las notas no pueden exceder 1000 caracteres")
                .When(x => !string.IsNullOrEmpty(x.NotasCliente));

            // ========== VALIDACIÓN MÉTODO DE PAGO ==========
            // COMENTADO TEMPORALMENTE - Se implementará con Wompi
            // RuleFor(x => x.MetodoPago)
            //     .NotEmpty()
            //     .WithMessage("Debe seleccionar un método de pago")
            //     .Must(metodo => new[] { "Efectivo", "Tarjeta", "PSE", "Nequi", "Daviplata" }
            //         .Contains(metodo))
            //     .WithMessage("Método de pago no válido");
        }
    }

    /// <summary>
    /// Validador para items individuales del pedido
    /// RN3 AJUSTADO: Límite de 10 unidades por producto
    /// </summary>
    public class PedidoItemRequestValidator : AbstractValidator<PedidoItemRequestDto>
    {
        public PedidoItemRequestValidator()
        {
            // ========== VALIDACIÓN PRODUCTO ID ==========
            RuleFor(x => x.ProductoId)
                .GreaterThan(0)
                .WithMessage("ID de producto inválido");

            // ========== VALIDACIÓN CANTIDAD (RN3 AJUSTADO: MÁXIMO 10) ==========
            RuleFor(x => x.Cantidad)
                .GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor a 0")
                .LessThanOrEqualTo(10)
                .WithMessage("No puede agregar más de 10 unidades por producto (RN3)");
        }
    }
}