using FluentValidation;
using PastisserieAPI.Services.DTOs.Request;

namespace PastisserieAPI.Services.Validators
{
    public class CreatePedidoRequestValidator : AbstractValidator<CreatePedidoRequestDto>
    {
        public CreatePedidoRequestValidator()
        {
            RuleFor(x => x.UsuarioId)
                .GreaterThan(0).WithMessage("El ID de usuario es inválido");

            RuleFor(x => x.MetodoPagoId)
                .GreaterThan(0).WithMessage("Debe seleccionar un método de pago");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("El pedido debe tener al menos un producto")
                .Must(items => items.Count <= 50).WithMessage("No puede agregar más de 50 productos al pedido");

            RuleForEach(x => x.Items).SetValidator(new PedidoItemRequestValidator());

            RuleFor(x => x.NotasCliente)
                .MaximumLength(1000).WithMessage("Las notas no pueden exceder 1000 caracteres")
                .When(x => !string.IsNullOrEmpty(x.NotasCliente));
        }
    }

    public class PedidoItemRequestValidator : AbstractValidator<PedidoItemRequestDto>
    {
        public PedidoItemRequestValidator()
        {
            RuleFor(x => x.ProductoId)
                .GreaterThan(0).WithMessage("El ID del producto es inválido");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
                .LessThanOrEqualTo(100).WithMessage("No puede ordenar más de 100 unidades por producto");
        }
    }
}