namespace MikroProje.Application.Features.Sales.DTOs;

public class SaleDetailDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string ProductCode { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal VatRate { get; set; }

    public decimal Discount { get; set; }

    public decimal LineTotal { get; set; }
}
