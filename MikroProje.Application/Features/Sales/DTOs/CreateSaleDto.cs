namespace MikroProje.Application.Features.Sales.DTOs;

public class CreateSaleDto
{
    public int CurrentAccountId { get; set; }

    public List<SaleItemDto> Items { get; set; } = new();

    public string? Description { get; set; }
}
