using System.Security.Claims;
using MikroProje.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MikroProje.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId
    {
        get
        {
            var id = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                     _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
            return string.IsNullOrEmpty(id) ? "System" : id;
        }
    }

    public string? Username
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "System";

            var name = context.User?.FindFirstValue(ClaimTypes.Email) 
                       ?? context.User?.FindFirstValue(ClaimTypes.Name)
                       ?? context.User?.FindFirstValue("name")
                       ?? context.User?.FindFirstValue("email");

            return string.IsNullOrEmpty(name) ? "Anonymous" : name;
        }
    }

    public string? IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                return forwardedFor.FirstOrDefault();
            }

            return context.Connection?.RemoteIpAddress?.ToString();
        }
    }

    public string? RequestPath => _httpContextAccessor.HttpContext?.Request?.Path.Value;

    public string? CorrelationId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return Guid.NewGuid().ToString();

            if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            {
                return correlationId.FirstOrDefault();
            }

            return context.TraceIdentifier;
        }
    }
}
