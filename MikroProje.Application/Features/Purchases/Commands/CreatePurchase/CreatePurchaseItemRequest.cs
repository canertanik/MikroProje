namespace MikroProje.Application.Features.Purchases.Commands.CreatePurchase;

public class CreatePurchaseItemRequest
{
    public int ProductId { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal? UnitPrice { get; set; }
}
