using FluentValidation;

namespace events_service.Application.Commands.PagarPublicacion
{
    /// <summary>
    /// Validador encargado de asegurar la consistencia de los datos del comando PagarPublicacionCommand.
    /// </summary>
    public class PagarPublicacionCommandValidator : AbstractValidator<PagarPublicacionCommand>
    {
        /// <summary>
        /// Inicializa las reglas de validación para el comando de pago de publicación.
        /// </summary>
        public PagarPublicacionCommandValidator()
        {
            RuleFor(command => command.EventoId)
                .NotEmpty()
                .WithMessage("El identificador del evento es requerido.");

            RuleFor(command => command.TransaccionPagoId)
                .NotEmpty()
                .WithMessage("La transacción de pago es requerida.");

            RuleFor(command => command.Monto)
                .GreaterThan(0)
                .WithMessage("El monto abonado debe ser mayor a cero.");
        }
    }
}
