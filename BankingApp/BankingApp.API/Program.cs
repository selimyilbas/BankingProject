using BankingApp.Infrastructure;
using BankingApp.Application;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.Services.Implementations;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(BankingApp.Application.Mappings.MappingProfile).Assembly);

// Add Infrastructure and Application services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

// Encryption service registration
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

// Add CORS
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

// Configure the HTTP request pipeline.
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
