using DFIR.CaseManagement.DTOs;
using FluentValidation;

namespace DFIR.CaseManagement.Validators;

public class EvidenceCreateDtoValidator : AbstractValidator<EvidenceCreateDto>
{
    public EvidenceCreateDtoValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0).WithMessage("A valid CaseId is required.");

        RuleFor(x => x.DeviceType)
            .NotEmpty().WithMessage("Device type is required.")
            .MaximumLength(100);

        RuleFor(x => x.Manufacturer).MaximumLength(100);
        RuleFor(x => x.Model).MaximumLength(100);
        RuleFor(x => x.SerialNumber).MaximumLength(100);
    }
}
