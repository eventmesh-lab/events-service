using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using events_service.Domain.Entities;
using events_service.Domain.Ports;

namespace events_service.Application.Commands.FinalizarEvento
{
    /// <summary>
    /// Handler encargado de marcar un evento en curso como finalizado.
    /// </summary>
    public class FinalizarEventoCommandHandler : IRequestHandler<FinalizarEventoCommand>
    {
        private readonly IEventoRepository _repository;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IValidator<FinalizarEventoCommand> _validator;

        /// <summary>
        /// Inicializa una nueva instancia del handler de finalización de eventos.
        /// </summary>
        public FinalizarEventoCommandHandler(
            IEventoRepository repository,
            IDomainEventPublisher domainEventPublisher,
            IValidator<FinalizarEventoCommand> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Maneja el comando FinalizarEventoCommand.
        /// </summary>
        public async Task Handle(FinalizarEventoCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var evento = await _repository.GetByIdAsync(request.EventoId, cancellationToken);
            if (evento is null)
            {
                throw new InvalidOperationException($"El evento con ID {request.EventoId} no existe.");
            }

            evento.Finalizar(DateTime.UtcNow);

            await _repository.UpdateAsync(evento, cancellationToken);
            await PublicarEventosDeDominio(evento, cancellationToken);
        }

        /// <summary>
        /// Publica los eventos de dominio generados y limpia la colección del agregado.
        /// </summary>
        private async Task PublicarEventosDeDominio(Evento evento, CancellationToken cancellationToken)
        {
            foreach (var domainEvent in evento.GetDomainEvents())
            {
                await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
            }

            evento.ClearDomainEvents();
        }
    }
}
