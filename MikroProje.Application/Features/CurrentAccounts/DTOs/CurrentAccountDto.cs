using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.CurrentAccounts.DTOs;

public class CurrentAccountDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? TaxNumber { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public CurrentAccountType Type { get; set; }

    public decimal Balance { get; set; }

    public DateTime CreatedDate { get; set; }
}
