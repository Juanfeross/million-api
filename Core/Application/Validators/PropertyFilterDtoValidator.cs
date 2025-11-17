using FluentValidation;
using Core.Application.DTOs;

namespace Core.Application.Validators;

/// <summary>
/// Validador para PropertyFilterDto
/// </summary>
public class PropertyFilterDtoValidator : AbstractValidator<PropertyFilterDto>
{
    /// <summary>
    /// Constructor que define las reglas de validación
    /// </summary>
    public PropertyFilterDtoValidator()
    {
        // MinPrice debe ser mayor o igual a 0 si se proporciona
        When(x => x.MinPrice.HasValue, () =>
        {
            RuleFor(x => x.MinPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El precio mínimo debe ser mayor o igual a 0");
        });

        // MaxPrice debe ser mayor o igual a 0 si se proporciona
        When(x => x.MaxPrice.HasValue, () =>
        {
            RuleFor(x => x.MaxPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El precio máximo debe ser mayor o igual a 0");
        });

        // MaxPrice debe ser mayor o igual a MinPrice si ambos se proporcionan
        When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue, () =>
        {
            RuleFor(x => x.MaxPrice)
                .GreaterThanOrEqualTo(x => x.MinPrice)
                .WithMessage("El precio máximo debe ser mayor o igual al precio mínimo");
        });
    }
}

