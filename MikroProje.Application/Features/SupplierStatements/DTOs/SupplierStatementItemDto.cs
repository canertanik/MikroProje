using System.Text.Json.Serialization;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.SupplierStatements.DTOs;

public class SupplierStatementItemDto
{
    public DateTime Date { get; set; }
    
    public DocumentType DocumentType { get; set; }
    
    public string DocumentTypeName => DocumentType.ToString();
    
    [JsonIgnore]
    public int DocumentId { get; set; }

    public string DocumentNumber => DocumentType == DocumentType.Purchase ? $"PURCHASE-{DocumentId}" : $"SPAY-{DocumentId}";
    
    public string Description { get; set; } = string.Empty;
    
    public decimal Debit { get; set; }
    
    public decimal Credit { get; set; }
    
    public decimal RunningBalance { get; set; }
}
