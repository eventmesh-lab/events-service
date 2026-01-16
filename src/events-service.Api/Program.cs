using events_service.Api.Configuration;
using events_service.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllers();
builder.Services.ConfigureSwagger();
builder.Services.ConfigureServices(builder.Configuration);

// Configurar CORS para permitir peticiones desde el frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Ejecutar migraciones automáticamente durante el arranque
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Aplicando migraciones de base de datos...");
        var context = services.GetRequiredService<EventsDbContext>();
        await context.Database.MigrateAsync();
        logger.LogInformation("Migraciones aplicadas correctamente.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Error al aplicar las migraciones de la base de datos.");
        throw;
    }
}

// Configurar pipeline HTTP
app.ConfigureMiddleware();

await app.RunAsync();
