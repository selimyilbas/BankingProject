/// <summary>
/// Program giriş noktası.
/// - DI kayıtları (Controller, Swagger, AutoMapper, Application/Infrastructure)
/// - Opsiyonel şifreleme servisi (AES-GCM)
/// - CORS (Angular 4200 için izin)
/// - HTTP middleware hattı (Swagger, HTTPS yönlendirme, CORS, Authorization, Controller haritalama)
/// </summary>
using BankingApp.Infrastructure;
using BankingApp.Application;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.Services.Implementations;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Hizmet kayıtları (Dependency Injection)
/// </summary>
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Banking API",
        Version = "v1",
        Description = "A comprehensive banking application API for VakıfBank"
    });
});

/// <summary>
/// Nesne eşleme (AutoMapper)
/// </summary>
builder.Services.AddAutoMapper(typeof(BankingApp.Application.Mappings.MappingProfile).Assembly);

/// <summary>
/// Katman bağımlılıkları (Infrastructure & Application)
/// </summary>
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

/// <summary>
/// Şifreleme servisi kaydı (opsiyonel)
/// </summary>
var encryptionKey = builder.Configuration["Encryption:Key"]; 
var encryptionVersion = builder.Configuration["Encryption:Version"] ?? "v1";
if (!string.IsNullOrWhiteSpace(encryptionKey))
{
    try
    {
        builder.Services.AddSingleton<IEncryptionService>(sp => new AesEncryptionService(encryptionKey, encryptionVersion));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Encryption service not configured due to key error: {ex.Message}");
    }
}

/// <summary>
/// CORS yapılandırması (Angular 4200 için)
/// </summary>
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

/// <summary>
/// HTTP istek hattı (middleware)
/// </summary>
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();
