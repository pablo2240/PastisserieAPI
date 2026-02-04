using FluentValidation;
using PastisserieAPI.Services.DTOs.Request;

namespace PastisserieAPI.Services.Validators
{
    public class CreateDireccionRequestValidator : AbstractValidator<CreateDireccionRequestDto>
    {
        public CreateDireccionRequestValidator()
        {
            RuleFor(x => x.NombreCompleto)
                .NotEmpty().WithMessage("El nombre completo es requerido")
                .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

            RuleFor(x => x.Direccion)
                .NotEmpty().WithMessage("La dirección es requerida")
                .MaximumLength(500).WithMessage("La dirección no puede exceder 500 caracteres");

            RuleFor(x => x.Barrio)
                .MaximumLength(100).WithMessage("El barrio no puede exceder 100 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Barrio));

            RuleFor(x => x.Referencia)
                .MaximumLength(500).WithMessage("La referencia no puede exceder 500 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Referencia));

            RuleFor(x => x.Telefono)
                .NotEmpty().WithMessage("El teléfono es requerido")
                .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
                .Matches(@"^\+?[\d\s\-()]+$").WithMessage("El formato del teléfono no es válido");
        }
    }
}