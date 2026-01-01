using FluentValidation;

namespace CleanArchitecture.Application.Features.Lectures.Commands.UpdateLecture
{
    public class UpdateLectureCommandValidator : AbstractValidator<UpdateLectureCommand>
    {
        public UpdateLectureCommandValidator()
        {
            RuleFor(p => p.Code)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(6).WithMessage("{PropertyName} must not exceed 6 characters.");
            RuleFor(p => p.Name)
                .NotNull().WithMessage("{PropertyName} should not be null.")
                .NotEmpty().WithMessage("{PropertyName} should not be empty.")
                .MaximumLength(100).WithMessage("{PropertyName} should not exceed {MaxLength} characters.");

            RuleFor(p => p.Description)
                .MaximumLength(500).WithMessage("{PropertyName} should not exceed {MaxLength} characters.");
        }
    }
}