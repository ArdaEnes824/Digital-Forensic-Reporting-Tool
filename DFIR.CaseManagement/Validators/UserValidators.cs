using DFIR.CaseManagement.DTOs;
using FluentValidation;

namespace DFIR.CaseManagement.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    private static readonly string[] AllowedRoles = { "Admin", "Analyst", "Viewer" };

    public RegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(4).WithMessage("Password must be at least 4 characters.");

        RuleFor(x => x.Role)
            .Must(role => AllowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Role must be one of: Admin, Analyst, Viewer.");
    }
}

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}
