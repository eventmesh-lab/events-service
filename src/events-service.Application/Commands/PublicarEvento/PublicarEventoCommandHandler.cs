using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using events_service.Domain.Events;
using events_service.Domain.Ports;

namespace events_service.Application.Commands.PublicarEvento
{
    /// <summary>
    /// Handler para el comando PublicarEventoCommand.
    /// Orquesta la publicación de un evento existente.
    /// </summary>
    public class PublicarEventoCommandHandler : IRequestHandler<PublicarEventoCommand>
    {
        private readonly IEventoRepository _repository;
        private readonly IPublisher _publisher;
        private readonly IValidator<PublicarEventoCommand> _validator;

        /// <summary>
        /// Inicializa una nueva instancia del handler.
        /// </summary>
        /// <param name="repository">Repositorio de eventos.</param>
        /// <param name="publisher">Publicador de eventos de dominio.</param>
        /// <param name="validator">Validador del comando.</param>
        public PublicarEventoCommandHandler(
            IEventoRepository repository,
            IPublisher publisher,
            IValidator<PublicarEventoCommand> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Maneja el comando PublicarEventoCommand.
        /// </summary>
        /// <param name="request">Comando a procesar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task Handle(PublicarEventoCommand request, CancellationToken cancellationToken)
        {
            // Validar el comando
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Obtener el evento
            var evento = await _repository.GetByIdAsync(request.EventoId, cancellationToken);
            if (evento == null)
            {
                throw new InvalidOperationException($"El evento con ID {request.EventoId} no existe.");
            }

            // Publicar el evento (el dominio valida el estado)
            evento.Publicar();

            // Persistir los cambios
            await _repository.UpdateAsync(evento, cancellationToken);

            // Publicar eventos de dominio
            var domainEvents = evento.GetDomainEvents();
            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
            evento.ClearDomainEvents();
        }
    }
}

