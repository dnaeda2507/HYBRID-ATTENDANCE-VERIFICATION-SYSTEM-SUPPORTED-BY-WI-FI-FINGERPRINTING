using FluentValidation;

namespace CleanArchitecture.Application.Features.Lectures.Commands.CreateLecture
{
    public class CreateLectureCommandValidator : AbstractValidator<CreateLectureCommand>
    {
        public CreateLectureCommandValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(6).WithMessage("Code must not exceed 6 characters.");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
        }
    }
}