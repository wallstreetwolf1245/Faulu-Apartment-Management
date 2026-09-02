using FluentValidation;
using FauluApartmentAPI.Models.Dtos;

namespace FauluApartmentAPI.Validators;

public class CreateBuildingValidator : AbstractValidator<CreateBuildingDto>
{
    public CreateBuildingValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Building name is required")
            .MaximumLength(255).WithMessage("Building name must not exceed 255 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.TotalUnits)
            .GreaterThan(0).WithMessage("Total units must be greater than 0");

        RuleFor(x => x.OwnerId)
            .GreaterThan(0).WithMessage("Owner ID must be valid");
    }
}

public class CreateUnitValidator : AbstractValidator<CreateUnitDto>
{
    public CreateUnitValidator()
    {
        RuleFor(x => x.UnitNumber)
            .NotEmpty().WithMessage("Unit number is required");

        RuleFor(x => x.BuildingId)
            .GreaterThan(0).WithMessage("Building ID must be valid");

        RuleFor(x => x.MonthlyRent)
            .GreaterThan(0).WithMessage("Monthly rent must be greater than 0");

        RuleFor(x => x.BedroomCount)
            .GreaterThanOrEqualTo(0).WithMessage("Bedroom count must be 0 or more");

        RuleFor(x => x.BathroomCount)
            .GreaterThanOrEqualTo(0).WithMessage("Bathroom count must be 0 or more");
    }
}

public class CreateTenantValidator : AbstractValidator<CreateTenantDto>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email is required");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[\d\s\-\(\)]{10,}$").WithMessage("Valid phone number is required");

        RuleFor(x => x.IdentificationNumber)
            .NotEmpty().WithMessage("Identification number is required");
    }
}

public class CreateLeaseValidator : AbstractValidator<CreateLeaseDto>
{
    public CreateLeaseValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("Tenant ID must be valid");

        RuleFor(x => x.UnitId)
            .GreaterThan(0).WithMessage("Unit ID must be valid");

        RuleFor(x => x.MonthlyRent)
            .GreaterThan(0).WithMessage("Monthly rent must be greater than 0");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Start date must be today or later");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}

public class CreatePaymentValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("Tenant ID must be valid");

        RuleFor(x => x.LeaseId)
            .GreaterThan(0).WithMessage("Lease ID must be valid");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[\d]").WithMessage("Password must contain at least one digit");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");
    }
}
