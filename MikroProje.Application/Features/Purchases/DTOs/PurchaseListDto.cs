namespace MikroProje.Application.Features.Purchases.DTOs;

public class PurchaseListDto
{
    public int Id { get; set; }
    public string CurrentAccountName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Description { get; set; }
}
