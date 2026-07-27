namespace MikroProje.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    string? IpAddress { get; }
    string? RequestPath { get; }
    string? CorrelationId { get; }
}
