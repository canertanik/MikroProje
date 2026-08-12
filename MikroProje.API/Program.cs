using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MikroProje.Application;
using MikroProje.Infrastructure;
using MikroProje.Persistence;
using MikroProje.API.Extensions;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCustomObservability(builder);

var healthChecksBuilder = builder.Services.AddHealthChecks()
    .AddCheck("live", () => HealthCheckResult.Healthy("Application is live"), tags: new[] { "live" })
    .AddDbContextCheck<MikroProje.Persistence.Contexts.MikroProjeDbContext>(
        name: "sqlserver",
        tags: new[] { "ready" });

var isRedisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled", true);
if (isRedisEnabled)
{
    healthChecksBuilder.AddRedis(
        builder.Configuration["Redis:ConnectionString"]!,
        name: "redis",
        tags: new[] { "ready" });
}
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Required for SSE
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MikroProje.Application.Interfaces.ICurrentUserService, MikroProje.API.Services.CurrentUserService>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MikroProje API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddScoped<MikroProje.Application.Interfaces.IApplicationDbContext>(provider => provider.GetRequiredService<MikroProje.Persistence.Contexts.MikroProjeDbContext>());
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCustomRateLimiting(builder.Configuration);

// JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
        ClockSkew = TimeSpan.Zero // Expire exactly when token expires
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Development ortamında sadece HTTPS tanımlıysa (veya Production/Docker ortamındaysa) yönlendirme yap
var urls = builder.Configuration["ASPNETCORE_URLS"];
var httpsPorts = builder.Configuration["ASPNETCORE_HTTPS_PORTS"];
var httpsPort = builder.Configuration["ASPNETCORE_HTTPS_PORT"];
var hasHttps = (urls != null && urls.Contains("https://", StringComparison.OrdinalIgnoreCase)) ||
               !string.IsNullOrEmpty(httpsPorts) ||
               !string.IsNullOrEmpty(httpsPort);

if (!app.Environment.IsDevelopment() || hasHttps)
{
    app.UseHttpsRedirection();
}

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        // Eğer exception Serilog'a kadar ulaştıysa (yakalanmadıysa) Error logla.
        if (ex != null) return LogEventLevel.Error;
        
        // Middleware tarafından yakalanıp 500 dönüldüyse, exception zaten middleware'de loglandı.
        // Serilog request özetini sadece Information veya Warning olarak loglasın, böylece duplicate Error oluşmaz.
        if (httpContext.Response.StatusCode > 499) return LogEventLevel.Warning;
        if (httpContext.Response.StatusCode > 399) return LogEventLevel.Warning;

        var path = httpContext.Request.Path.Value;
        if (path != null && (path.StartsWith("/swagger") || path.StartsWith("/health")))
        {
            return LogEventLevel.Debug; // Filtrele
        }

        return LogEventLevel.Information;
    };
});

app.UseMiddleware<MikroProje.API.Middlewares.GlobalExceptionMiddleware>();

app.UseRouting();
app.UseRateLimiter();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
var healthCheckOptions = new HealthCheckOptions
{
    ResponseWriter = MikroProje.API.Extensions.HealthCheckExtensions.WriteResponse
};

var liveCheckOptions = new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = MikroProje.API.Extensions.HealthCheckExtensions.WriteResponse
};

var readyCheckOptions = new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = MikroProje.API.Extensions.HealthCheckExtensions.WriteResponse
};

app.MapHealthChecks("/health", healthCheckOptions).AllowAnonymous().DisableRateLimiting();
app.MapHealthChecks("/health/live", liveCheckOptions).AllowAnonymous().DisableRateLimiting();
app.MapHealthChecks("/health/ready", readyCheckOptions).AllowAnonymous().DisableRateLimiting();

if (builder.Configuration["Database:ApplyMigrations"] == "true")
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<MikroProje.Persistence.Contexts.MikroProjeDbContext>();
        
        if (context.Database.IsSqlServer())
        {
            int retries = 5;
            int delayMs = 2000;
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Database migration applied successfully.");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Migration attempt {Attempt} failed. Retrying in {Delay}ms...", i + 1, delayMs);
                    if (i == retries - 1)
                    {
                        logger.LogError(ex, "All migration attempts failed.");
                        throw;
                    }
                    await Task.Delay(delayMs);
                }
            }
        }
    }
}

app.Run();

public partial class Program { }
