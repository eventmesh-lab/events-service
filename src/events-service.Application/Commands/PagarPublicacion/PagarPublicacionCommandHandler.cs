using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using events_service.Domain.Entities;
using events_service.Domain.Ports;

namespace events_service.Application.Commands.PagarPublicacion
{
    /// <summary>
    /// Handler encargado de iniciar el proceso de pago de publicación de un evento.
    /// </summary>
    public class PagarPublicacionCommandHandler : IRequestHandler<PagarPublicacionCommand>
    {
        private readonly IEventoRepository _repository;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IValidator<PagarPublicacionCommand> _validator;

        /// <summary>
        /// Inicializa una nueva instancia del handler de pagos.
        /// </summary>
        /// <param name="repository">Repositorio de eventos.</param>
        /// <param name="domainEventPublisher">Publicador de eventos de dominio.</param>
        /// <param name="validator">Validador del comando.</param>
        public PagarPublicacionCommandHandler(
            IEventoRepository repository,
            IDomainEventPublisher domainEventPublisher,
            IValidator<PagarPublicacionCommand> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Maneja el comando PagarPublicacionCommand.
        /// </summary>
        /// <param name="request">Información del pago.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task Handle(PagarPublicacionCommand request, CancellationToken cancellationToken)
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

            evento.PagarPublicacion(request.TransaccionPagoId, request.Monto);

            await _repository.UpdateAsync(evento, cancellationToken);
            await PublicarEventosDeDominio(evento, cancellationToken);
        }

        /// <summary>
        /// Publica los eventos de dominio generados y limpia la cola del agregado.
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
