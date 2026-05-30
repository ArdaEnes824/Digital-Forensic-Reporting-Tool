using DFIR.CaseManagement.DTOs;
using FluentValidation;

namespace DFIR.CaseManagement.Validators;

public class CaseCreateDtoValidator : AbstractValidator<CaseCreateDto>
{
    public CaseCreateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value.");

        RuleFor(x => x.AssignedTo)
            .MaximumLength(100);
    }
}

public class CaseUpdateDtoValidator : AbstractValidator<CaseUpdateDto>
{
    public CaseUpdateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value.");

        RuleFor(x => x.AssignedTo)
            .MaximumLength(100);
    }
}
