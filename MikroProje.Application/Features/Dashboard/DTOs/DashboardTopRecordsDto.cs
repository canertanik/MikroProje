namespace MikroProje.Application.Features.Dashboard.DTOs;

public class DashboardTopRecordsDto
{
    public List<TopCustomerDto> TopCustomersBySales { get; set; } = new();
    public List<TopProductDto> TopProductsBySales { get; set; } = new();
    public List<TopDebtorDto> TopDebtors { get; set; } = new();
    public List<TopCreditorDto> TopCreditors { get; set; } = new();
}

public class TopCustomerDto
{
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
}

public class TopProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
}

public class TopDebtorDto
{
    public string CustomerName { get; set; } = string.Empty;
    public decimal DebtAmount { get; set; }
}

public class TopCreditorDto
{
    public string SupplierName { get; set; } = string.Empty;
    public decimal CreditAmount { get; set; }
}
