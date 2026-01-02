using FluentValidation;
using MovieManager.Application.DTOs.Movies;

namespace MovieManager.Application.Validators;

public class MovieCreateValidator : AbstractValidator<MovieCreateDto>
{
    public MovieCreateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres");

        RuleFor(x => x.Year)
            .InclusiveBetween(1888, DateTime.Now.Year + 5)
            .WithMessage($"El año debe estar entre 1888 y {DateTime.Now.Year + 5}");

        RuleFor(x => x.Director)
            .MaximumLength(100).WithMessage("El director no puede exceder 100 caracteres");

        RuleFor(x => x.Rating)
            .InclusiveBetween(0, 10).WithMessage("La calificación debe estar entre 0 y 10");

        RuleFor(x => x.Duration)
            .GreaterThanOrEqualTo(0).WithMessage("La duración no puede ser negativa");
    }
}
