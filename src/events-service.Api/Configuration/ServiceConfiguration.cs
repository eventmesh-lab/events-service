using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using events_service.Application.Commands.CrearEvento;
using events_service.Application.Commands.PublicarEvento;
using events_service.Domain.Ports;
using events_service.Infrastructure.Messaging;
using events_service.Infrastructure.Persistence;
using events_service.Infrastructure.Repositories;
using events_service.Infrastructure.Storage;
using events_service.Infrastructure.ExternalServices;

namespace events_service.Api.Configuration;

/// <summary>
/// Configuración de servicios de la aplicación.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Configura todos los servicios de la aplicación.
    /// </summary>
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar Entity Framework Core con PostgreSQL
        services.AddDbContext<EventsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("EventsDb"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("events-service.Infrastructure")));

        // Registrar MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.RegisterServicesFromAssembly(typeof(CrearEventoCommand).Assembly);
        });

        // Registrar FluentValidation
        services.AddValidatorsFromAssembly(typeof(CrearEventoCommandValidator).Assembly);
        services.AddValidatorsFromAssembly(typeof(PublicarEventoCommandValidator).Assembly);

        // Registrar repositorios
        services.AddScoped<IEventoRepository, EventoRepository>();

        // Registrar mensajería RabbitMQ
        services.AddRabbitMqMessaging(configuration);

        // Configurar Firebase Storage (Almacenamiento de Blobs)
        services.Configure<FirebaseStorageOptions>(configuration.GetSection("FirebaseStorage"));
        services.AddSingleton<IEventMediaStorage, FirebaseEventMediaStorage>();

        // Registrar clientes de servicios externos
        services.AddHttpClient<IRegistrationClient, RegistrationServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:RegistrationServiceUrl"] ?? "http://registration-service");
        });

        // Configurar CORS para permitir peticiones desde el frontend
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        return services;
    }
}
