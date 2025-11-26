using FluentValidation;

namespace events_service.Application.Commands.FinalizarEvento
{
    /// <summary>
    /// Validador que comprueba los datos del comando FinalizarEventoCommand.
    /// </summary>
    public class FinalizarEventoCommandValidator : AbstractValidator<FinalizarEventoCommand>
    {
        /// <summary>
        /// Configura las reglas de validación para finalizar un evento.
        /// </summary>
        public FinalizarEventoCommandValidator()
        {
            RuleFor(command => command.EventoId)
                .NotEmpty()
                .WithMessage("El identificador del evento es requerido.");
        }
    }
}
