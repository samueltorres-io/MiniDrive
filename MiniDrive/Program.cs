using Microsoft.EntityFrameworkCore;
using Minio;
using MiniDrive.Data;
using MiniDrive.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
        .UseSnakeCaseNamingConvention());

static string? GetFirstNonEmpty(IConfiguration config, params string[] keys)
{
    foreach (var key in keys)
    {
        var value = config[key];
        if (!string.IsNullOrWhiteSpace(value))
            return value;
    }

    return null;
}

var minioEndpoint = GetFirstNonEmpty(builder.Configuration, "Minio__Endpoint", "Minio:Endpoint", "MINIO_ENDPOINT");
var minioAccess = GetFirstNonEmpty(builder.Configuration, "Minio__AccessKey", "Minio:AccessKey", "MINIO_ACCESS_KEY");
var minioSecret = GetFirstNonEmpty(builder.Configuration, "Minio__SecretKey", "Minio:SecretKey", "MINIO_SECRET_KEY");
var minioUseSslRaw = GetFirstNonEmpty(builder.Configuration, "Minio__UseSSL", "Minio:UseSSL", "MINIO_USE_SSL");

if (string.IsNullOrWhiteSpace(minioEndpoint) ||
    string.IsNullOrWhiteSpace(minioAccess) ||
    string.IsNullOrWhiteSpace(minioSecret))
{
    if (builder.Environment.IsDevelopment())
    {
        // Local dev defaults (works with a vanilla `minio/minio` container on localhost).
        minioEndpoint = "http://localhost:9000";
        minioAccess = "minioadmin";
        minioSecret = "minioadmin";
        minioUseSslRaw ??= "false";
    }
    else
    {
        throw new InvalidOperationException(
            "MinIO configuration is missing. Set either Minio__Endpoint/Minio__AccessKey/Minio__SecretKey " +
            "or Minio:Endpoint/Minio:AccessKey/Minio:SecretKey (or env vars MINIO_ENDPOINT/MINIO_ACCESS_KEY/MINIO_SECRET_KEY).");
    }
}

var minioUseSsl =
    bool.TryParse(minioUseSslRaw, out var parsedUseSsl) ? parsedUseSsl : false;

// If endpoint includes scheme, infer SSL unless explicitly configured.
if (Uri.TryCreate(minioEndpoint, UriKind.Absolute, out var minioUri))
{
    if (string.IsNullOrWhiteSpace(minioUseSslRaw))
        minioUseSsl = string.Equals(minioUri.Scheme, "https", StringComparison.OrdinalIgnoreCase);

    minioEndpoint = string.IsNullOrWhiteSpace(minioUri.Port.ToString())
        ? minioUri.Host
        : $"{minioUri.Host}:{minioUri.Port}";
}
else
{
    minioEndpoint = minioEndpoint
        .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
        .Replace("https://", "", StringComparison.OrdinalIgnoreCase);
}

// var minioAccess   = builder.Configuration["Minio__AccessKey"]!;
// var minioSecret   = builder.Configuration["Minio__SecretKey"]!;

builder.Services.AddMinio(cfg => cfg
    .WithEndpoint(minioEndpoint)
    .WithCredentials(minioAccess, minioSecret)
    .WithSSL(minioUseSsl)
    .Build());

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                  "http://localhost:4200",
                  "http://127.0.0.1:4200",
                  "http://0.0.0.0:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    app.MapOpenApi();
}

app.UseCors();

app.MapControllers();

app.Run();