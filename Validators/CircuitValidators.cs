using FluentValidation;
using LogicLink.Api.DTOs;

namespace LogicLink.Api.Validators;

public class CreateCircuitRequestValidator : AbstractValidator<CreateCircuitRequest>
{
    public CreateCircuitRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Circuit name is required.")
            .MaximumLength(120);

        RuleFor(x => x.OwnerName)
            .NotEmpty().WithMessage("Owner name is required.")
            .MaximumLength(60);

        RuleFor(x => x.GridSize)
            .InclusiveBetween(10, 100)
            .When(x => x.GridSize.HasValue)
            .WithMessage("Grid size must be between 10 and 100 px.");
    }
}

public class UpdateCircuitSettingsRequestValidator : AbstractValidator<UpdateCircuitSettingsRequest>
{
    public UpdateCircuitSettingsRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Circuit name is required.")
            .MaximumLength(120);

        RuleFor(x => x.GridSize)
            .InclusiveBetween(10, 100)
            .WithMessage("Grid size must be between 10 and 100 px.");
    }
}