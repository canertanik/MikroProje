using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.CurrentAccounts.DTOs;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.CurrentAccounts.Commands.UpdateCurrentAccount;

public class UpdateCurrentAccountCommand : IRequest<Result<CurrentAccountDto>>
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? TaxNumber { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public CurrentAccountType Type { get; set; }
}
