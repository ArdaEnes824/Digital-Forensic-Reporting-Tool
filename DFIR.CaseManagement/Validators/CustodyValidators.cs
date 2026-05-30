using DFIR.CaseManagement.DTOs;
using FluentValidation;

namespace DFIR.CaseManagement.Validators;

public class CustodyCreateDtoValidator : AbstractValidator<CustodyCreateDto>
{
    public CustodyCreateDtoValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0).WithMessage("A valid CaseId is required.");

        RuleFor(x => x.FromPerson)
            .NotEmpty().WithMessage("FromPerson is required.")
            .MaximumLength(100);

        RuleFor(x => x.ToPerson)
            .NotEmpty().WithMessage("ToPerson is required.")
            .MaximumLength(100);

        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
