using MikroProje.Domain.Entities;

namespace MikroProje.Application.Interfaces;

public interface ISaleRepository
{
    Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<(IReadOnlyCollection<Sale> Items, int TotalCount)> GetAllAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<(IReadOnlyCollection<Sale> Items, int TotalCount)> GetByCurrentAccountAsync(
        int currentAccountId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Sale + SaleDetail + StockMovement (StockOut) + Product stok + CurrentAccount.Balance
    /// işlemlerini tek transaction içinde gerçekleştirir.
    /// </summary>
    Task<Sale> CreateSaleAsync(
        Sale sale,
        List<SaleLineItem> lineItems,
        CurrentAccount currentAccount,
        CancellationToken cancellationToken);

    /// <summary>
    /// Yalnızca Sale header (Description gibi) günceller.
    /// </summary>
    Task<Sale> UpdateSaleAsync(Sale sale, CancellationToken cancellationToken);

    /// <summary>
    /// Soft delete + StockIn (ters hareket) + Product stok geri + CurrentAccount.Balance düşür.
    /// Tek transaction.
    /// </summary>
    Task CancelSaleAsync(Sale sale, CancellationToken cancellationToken);
}

/// <summary>
/// CreateSaleAsync'e iletilen satır verisi; Product nesnesi zaten yüklenmiş hâlde gelir.
/// </summary>
public record SaleLineItem(
    Product Product,
    int Quantity,
    decimal UnitPrice,
    decimal Discount);
