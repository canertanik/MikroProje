namespace MikroProje.Application.Common.Caching;

public static class CacheKeys
{
    // Prefixes
    public const string DashboardPrefix = "dashboard:";
    public const string ProductsPrefix = "products:";
    public const string CurrentAccountsPrefix = "currentaccounts:";
    public const string WarehousesPrefix = "warehouses:";
    public const string SupplierPaymentsPrefix = "supplierpayments:";

    // Dashboard
    public static string DashboardSummary() => $"{DashboardPrefix}summary";

    // Products
    public static string ProductById(int id) => $"{ProductsPrefix}{id}";
    public static string ProductList(int page, int size, string? search, string? filter, string? sort, string? dir)
    {
        return BuildListKey(ProductsPrefix, page, size, search, filter, sort, dir);
    }

    // Current Accounts
    public static string CurrentAccountById(int id) => $"{CurrentAccountsPrefix}{id}";
    public static string CurrentAccountList(int page, int size, string? search, string? filter, string? sort, string? dir)
    {
        return BuildListKey(CurrentAccountsPrefix, page, size, search, filter, sort, dir);
    }

    // Warehouses
    public static string WarehouseById(int id) => $"{WarehousesPrefix}{id}";
    public static string WarehouseList(int page, int size, string? search, string? filter, string? sort, string? dir)
    {
        return BuildListKey(WarehousesPrefix, page, size, search, filter, sort, dir);
    }

    // Supplier Payments
    public static string SupplierPaymentById(int id) => $"{SupplierPaymentsPrefix}{id}";
    public static string SupplierPaymentList(int page, int size, string? search, string? filter, string? sort, string? dir)
    {
        return BuildListKey(SupplierPaymentsPrefix, page, size, search, filter, sort, dir);
    }

    private static string BuildListKey(string prefix, int page, int size, string? search, string? filter, string? sort, string? dir)
    {
        var nSearch = search?.Trim().ToLowerInvariant() ?? "";
        var nFilter = filter?.Trim().ToLowerInvariant() ?? "";
        var nSort = sort?.Trim().ToLowerInvariant() ?? "";
        var nDir = dir?.Trim().ToLowerInvariant() ?? "";

        return $"{prefix}list:page={page}:size={size}:search={nSearch}:filter={nFilter}:sort={nSort}:dir={nDir}";
    }
}
