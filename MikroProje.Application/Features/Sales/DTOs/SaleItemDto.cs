namespace MikroProje.Application.Features.Sales.DTOs;

public class SaleItemDto
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    /// <summary>
    /// İndirim oranı (%). 0–100 arası. Varsayılan: 0
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// Birim fiyat. Gönderilmezse Product.SalePrice kullanılır.
    /// </summary>
    public decimal? UnitPrice { get; set; }
}
