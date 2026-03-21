using FluentValidation;
using PastisserieAPI.Services.DTOs.Request;

namespace PastisserieAPI.Services.Validators
{
    public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequestDto>
    {
        public CreateReviewRequestValidator()
        {
            RuleFor(x => x.ProductoId)
                .GreaterThan(0).WithMessage("El ID del producto es inválido");

            RuleFor(x => x.Calificacion)
                .InclusiveBetween(1, 5).WithMessage("La calificación debe estar entre 1 y 5");

            RuleFor(x => x.Comentario)
                .MaximumLength(1000).WithMessage("El comentario no puede exceder 1000 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Comentario));
        }
    }
}