namespace MikroProje.Application.Common.Caching;

public class RedisOptions
{
    public const string SectionName = "Redis";

    public bool Enabled { get; set; } = true;
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
    public int DefaultExpirationMinutes { get; set; } = 10;
    
    public int DashboardExpirationMinutes { get; set; } = 5;
    public int ProductExpirationMinutes { get; set; } = 10;
    public int CurrentAccountExpirationMinutes { get; set; } = 10;
    public int WarehouseExpirationMinutes { get; set; } = 15;
    public int SupplierPaymentExpirationMinutes { get; set; } = 10;
}
