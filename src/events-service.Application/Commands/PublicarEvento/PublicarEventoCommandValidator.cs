using FluentValidation;

namespace events_service.Application.Commands.PublicarEvento
{
    /// <summary>
    /// Validador para el comando PublicarEventoCommand.
    /// Valida que el ID del evento sea válido.
    /// </summary>
    public class PublicarEventoCommandValidator : AbstractValidator<PublicarEventoCommand>
    {
        /// <summary>
        /// Inicializa una nueva instancia del validador con las reglas de validación.
        /// </summary>
        public PublicarEventoCommandValidator()
        {
            RuleFor(x => x.EventoId)
                .NotEmpty()
                .WithMessage("El ID del evento es requerido.");

            RuleFor(x => x.PagoConfirmadoId)
                .NotEmpty()
                .WithMessage("El ID del pago confirmado es requerido.");
        }
    }
}

