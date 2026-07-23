using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.CurrentAccounts.DTOs;

public record StatementDto
{
    public DateTime Date { get; init; }
    public DocumentType DocumentType { get; init; }
    public string DocumentTypeName => DocumentType.ToString();
    public int DocumentId { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public decimal BalanceAfterTransaction { get; init; }
}
