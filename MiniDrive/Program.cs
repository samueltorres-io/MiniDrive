using Microsoft.EntityFrameworkCore;
using MiniDrive.Data;
using MiniDrive.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddMinio(opts =>
{
    opts.Endpoint  = builder.Configuration["Minio__Endpoint"]!
                             .Replace("http://", "").Replace("https://", "");
    opts.AccessKey = builder.Configuration["Minio__AccessKey"]!;
    opts.SecretKey = builder.Configuration["Minio__SecretKey"]!;
    opts.WithSSL(false);
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
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