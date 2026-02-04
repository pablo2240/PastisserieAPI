using FluentValidation;
using PastisserieAPI.Services.DTOs.Request;

namespace PastisserieAPI.Services.Validators
{
    public class AddToCarritoRequestValidator : AbstractValidator<AddToCarritoRequestDto>
    {
        public AddToCarritoRequestValidator()
        {
            RuleFor(x => x.ProductoId)
                .GreaterThan(0).WithMessage("El ID del producto es inválido");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
                .LessThanOrEqualTo(100).WithMessage("No puede agregar más de 100 unidades");
        }
    }
}