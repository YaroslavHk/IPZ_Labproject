

using FluentValidation;
using RentalServer.DTO;

namespace RentalServer.Middlewares;

public class CreateRentalRequestValidator : AbstractValidator<RentalRequest>
{
    public CreateRentalRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title cannot be empty")
            .MinimumLength(5).WithMessage("Title is too short");
                             
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than zero");
        
        RuleFor(x => x.City).NotEmpty().WithMessage("City is required");
        
        RuleFor(x => x.LivingSpace).GreaterThan(10).WithMessage("Living space must be greater than 10 sq.m.");
    }
}