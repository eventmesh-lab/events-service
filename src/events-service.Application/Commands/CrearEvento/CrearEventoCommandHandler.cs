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
using events_service.Infrastructure.Messaging;

namespace events_service.Application.Commands.CrearEvento
{
    /// <summary>
    /// Handler para el comando CrearEventoCommand.
    /// Orquesta la creación de un nuevo evento en estado borrador.
    /// </summary>
    public class CrearEventoCommandHandler : IRequestHandler<CrearEventoCommand, CrearEventoCommandResponse>
    {
        private readonly IEventoRepository _repository;
        private readonly IPublisher _publisher;
        private readonly IValidator<CrearEventoCommand> _validator;

        /// <summary>
        /// Inicializa una nueva instancia del handler.
        /// </summary>
        /// <param name="repository">Repositorio de eventos.</param>
        /// <param name="publisher">Publicador de eventos de dominio.</param>
        /// <param name="validator">Validador del comando.</param>
        public CrearEventoCommandHandler(
            IEventoRepository repository,
            IPublisher publisher,
            IValidator<CrearEventoCommand> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
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
            var secciones = request.Secciones.Select(s => 
                new Seccion(s.Nombre, s.Capacidad, new PrecioEntrada(s.Precio))
            ).ToList();

            // Crear evento usando el método factory del dominio
            var evento = Evento.Crear(
                request.Nombre,
                request.Descripcion,
                request.Fecha,
                duracion,
                secciones
            );

            // Persistir el evento
            await _repository.AddAsync(evento, cancellationToken);

            // Publicar eventos de dominio
            var domainEvents = evento.GetDomainEvents();
            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
            evento.ClearDomainEvents();

            return new CrearEventoCommandResponse { Id = evento.Id };
        }
    }
}

