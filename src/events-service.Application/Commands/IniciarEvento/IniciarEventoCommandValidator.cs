using FluentValidation;

namespace events_service.Application.Commands.IniciarEvento
{
    /// <summary>
    /// Validador que regula los datos del comando IniciarEventoCommand antes de su procesamiento.
    /// </summary>
    public class IniciarEventoCommandValidator : AbstractValidator<IniciarEventoCommand>
    {
        /// <summary>
        /// Configura las reglas de validación necesarias para iniciar un evento.
        /// </summary>
        public IniciarEventoCommandValidator()
        {
            RuleFor(command => command.EventoId)
                .NotEmpty()
                .WithMessage("El identificador del evento es requerido.");
        }
    }
}
