namespace MikroProje.Application.Features.Warehouses.DTOs;

public class WarehouseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
