using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using events_service.Domain.Entities;
using events_service.Domain.Events;
using events_service.Domain.Ports;
using events_service.Domain.ValueObjects;

namespace events_service.Application.Commands.CrearEvento
{
    /// <summary>
    /// Handler para el comando CrearEventoCommand.
    /// Orquesta la creación de un nuevo evento en estado borrador.
    /// </summary>
    public class CrearEventoCommandHandler : IRequestHandler<CrearEventoCommand, CrearEventoCommandResponse>
    {
        private readonly IEventoRepository _repository;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IValidator<CrearEventoCommand> _validator;

        /// <summary>
        /// Inicializa una nueva instancia del handler.
        /// </summary>
        /// <param name="repository">Repositorio de eventos.</param>
        /// <param name="domainEventPublisher">Publicador de eventos de dominio.</param>
        /// <param name="validator">Validador del comando.</param>
        public CrearEventoCommandHandler(
            IEventoRepository repository,
            IDomainEventPublisher domainEventPublisher,
            IValidator<CrearEventoCommand> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Maneja el comando CrearEventoCommand.
        /// </summary>
        /// <param name="request">Comando a procesar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Respuesta con el ID del evento creado.</returns>
        public async Task<CrearEventoCommandResponse> Handle(CrearEventoCommand request, CancellationToken cancellationToken)
        {
            // Validar el comando
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Mapear comando a entidades del dominio
            var duracion = new DuracionEvento(request.HorasDuracion, request.MinutosDuracion);
            var fechaEvento = new FechaEvento(request.Fecha);
            var secciones = request.Secciones.Select(s =>
                new Seccion(s.Nombre, s.Capacidad, new PrecioEntrada(s.Precio))
            ).ToList();

            // Crear evento usando el método factory del dominio
            var evento = Evento.Crear(
                request.Nombre,
                request.Descripcion,
                fechaEvento,
                duracion,
                request.OrganizadorId,
                request.VenueId,
                request.Categoria,
                request.TarifaPublicacion,
                secciones
            );

            // Persistir el evento
            await _repository.AddAsync(evento, cancellationToken);

            // Publicar eventos de dominio
            var domainEvents = evento.GetDomainEvents();
            foreach (var domainEvent in domainEvents)
            {
                await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
            }
            evento.ClearDomainEvents();

            return new CrearEventoCommandResponse { Id = evento.Id };
        }
    }
}

