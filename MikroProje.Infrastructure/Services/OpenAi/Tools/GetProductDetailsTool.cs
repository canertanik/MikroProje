using System.Text.Json;
using MikroProje.Application.Interfaces;

namespace MikroProje.Infrastructure.Services.OpenAi.Tools;

public class GetProductDetailsTool : IErpToolHandler
{
    private readonly IProductRepository _productRepository;

    public GetProductDetailsTool(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public string ToolName => "get_product_details";

    public string Description => "Belirli bir ürünün detaylarını (isim, kod, stok miktarı) getirir.";

    public object ParametersSchema => System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>("""
    {
        "type": "object",
        "properties": {
            "productId": {
                "type": "integer",
                "description": "Detayı getirilecek ürünün ID'si"
            }
        },
        "required": ["productId"],
        "additionalProperties": false
    }
    """);

    private class ToolArgs
    {
        public int ProductId { get; set; }
    }

    public async Task<string> ExecuteAsync(string argumentsJson, string userId, CancellationToken ct)
    {
        try
        {
            var args = JsonSerializer.Deserialize<ToolArgs>(argumentsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (args == null || args.ProductId <= 0)
            {
                return JsonSerializer.Serialize(new { error = "Geçerli bir productId (sıfırdan büyük) sağlanmalıdır." });
            }

            var product = await _productRepository.GetByIdAsync(args.ProductId, ct);
            if (product == null)
            {
                return JsonSerializer.Serialize(new { error = "Ürün bulunamadı." });
            }

            // Minimal DTO
            var result = new
            {
                product.Id,
                product.Code,
                product.Name,
                product.StockQuantity,
                product.CriticalStockQuantity,
                IsCriticalStock = product.StockQuantity < product.CriticalStockQuantity
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
