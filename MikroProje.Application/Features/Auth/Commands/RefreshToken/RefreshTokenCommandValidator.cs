using FluentValidation;

namespace MikroProje.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Request.AccessToken)
            .NotEmpty().WithMessage("Access Token boş olamaz.");

        RuleFor(x => x.Request.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token boş olamaz.");
    }
}
