using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Auth.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
{
    private readonly IAppUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IAppUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var isUnique = await _userRepository.IsEmailUniqueAsync(request.Request.Email, cancellationToken);
        if (!isUnique)
        {
            return Result<UserDto>.Fail("Bu email adresi zaten kullanılıyor.", 409); // 409 Conflict
        }

        var user = new AppUser
        {
            FirstName = request.Request.FirstName,
            LastName = request.Request.LastName,
            Email = request.Request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Request.Password),
            Role = UserRole.User, // Always default to User
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role
        };

        return Result<UserDto>.Created(userDto, "Kullanıcı başarıyla oluşturuldu.");
    }
}
