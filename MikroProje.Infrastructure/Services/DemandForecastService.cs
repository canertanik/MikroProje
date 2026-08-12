using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;

namespace MikroProje.Infrastructure.Services;

public class DemandForecastService : IDemandForecastService
{
    private readonly HttpClient _httpClient;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DemandForecastService> _logger;
    private readonly IProductRepository _productRepository;

    public DemandForecastService(
        HttpClient httpClient, 
        IApplicationDbContext context, 
        ILogger<DemandForecastService> logger,
        IProductRepository productRepository)
    {
        _httpClient = httpClient;
        _context = context;
        _logger = logger;
        _productRepository = productRepository;
    }

    public async Task<Result<ForecastResultDto>> GetProductForecastAsync(int productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                return Result<ForecastResultDto>.Fail("Product not found", 404);
            }

            // Get current total stock
            var stockList = await _productRepository.GetWarehouseStocksAsync(productId, cancellationToken);
            int currentStock = stockList.Sum(s => s.Quantity);

            // Fetch historical sales for this product grouped by date
            var salesData = await _context.Sales
                .Where(s => !s.IsDeleted)
                .SelectMany(s => s.Details)
                .Where(d => !d.IsDeleted && d.ProductId == productId)
                .GroupBy(d => d.Sale.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderBy(x => x.Date)
                .ToListAsync(cancellationToken);

            var requestPayload = new ForecastRequest
            {
                ProductId = productId,
                CurrentStock = currentStock,
                Sales = salesData.Select(x => new SaleDataRequest
                {
                    Date = x.Date.ToString("yyyy-MM-dd"),
                    Quantity = x.Quantity
                }).ToList()
            };

            // Call Python FastAPI
            var response = await _httpClient.PostAsJsonAsync("/api/forecast", requestPayload, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ForecastResultDto>(cancellationToken: cancellationToken);
            if (result == null)
            {
                return Result<ForecastResultDto>.Fail("Failed to deserialize ML response", 500);
            }

            return Result<ForecastResultDto>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting demand forecast for product {ProductId}", productId);
            return Result<ForecastResultDto>.Fail($"ML Service error: {ex.Message}", 500);
        }
    }
}

public class SaleDataRequest
{
    public string Date { get; set; } = string.Empty;
    public float Quantity { get; set; }
}

public class ForecastRequest
{
    public int ProductId { get; set; }
    public int CurrentStock { get; set; }
    public List<SaleDataRequest> Sales { get; set; } = new();
}
