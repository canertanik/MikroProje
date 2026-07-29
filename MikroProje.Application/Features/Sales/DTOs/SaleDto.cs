namespace MikroProje.Application.Features.Sales.DTOs;

public class SaleDto
{
    public int Id { get; set; }

    public int CurrentAccountId { get; set; }

    public string CurrentAccountName { get; set; } = string.Empty;

    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string CurrentAccountCode { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal VatAmount { get; set; }

    public decimal GrandTotal { get; set; }

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public List<SaleDetailDto> Items { get; set; } = new();
}
