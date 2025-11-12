using System;
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using events_service.Application.Commands.CrearEvento;
using events_service.Application.Commands.PublicarEvento;
using events_service.Domain.Ports;
using events_service.Infrastructure.Messaging;
using events_service.Infrastructure.Persistence;
using events_service.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Events Service API",
        Version = "v1",
        Description = "API para la gestión del ciclo de vida completo de eventos"
    });
});

// Configurar Entity Framework Core con PostgreSQL
builder.Services.AddDbContext<EventsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("EventsDb"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("events-service.Infrastructure")));

// Registrar MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CrearEventoCommand).Assembly));

// Registrar FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CrearEventoCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(PublicarEventoCommandValidator).Assembly);

// Registrar repositorios
builder.Services.AddScoped<IEventoRepository, EventoRepository>();

// Registrar mensajería RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration);

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Events Service API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Mapear endpoints
app.MapControllers();
app.MapEventosEndpoints();

app.Run();

// Extensiones para endpoints
public static class EventosEndpointsExtensions
{
    public static void MapEventosEndpoints(this WebApplication app)
    {
        var eventos = app.MapGroup("/api/eventos").WithTags("Eventos");

        // POST /api/eventos
        eventos.MapPost("/", async (CrearEventoCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Created($"/api/eventos/{result.Id}", result);
        })
        .WithName("CrearEvento")
        .WithSummary("Crea un nuevo evento en estado borrador")
        .Produces<CrearEventoCommandResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // POST /api/eventos/{id}/publicar
        eventos.MapPost("/{id:guid}/publicar", async (Guid id, IMediator mediator) =>
        {
            var command = new PublicarEventoCommand { EventoId = id };
            await mediator.Send(command);
            return Results.Ok();
        })
        .WithName("PublicarEvento")
        .WithSummary("Publica un evento existente")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/eventos/{id}
        eventos.MapGet("/{id:guid}", async (Guid id, IEventoRepository repository) =>
        {
            var evento = await repository.GetByIdAsync(id);
            if (evento == null)
                return Results.NotFound();
            return Results.Ok(evento);
        })
        .WithName("ObtenerEvento")
        .WithSummary("Obtiene los detalles de un evento")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
