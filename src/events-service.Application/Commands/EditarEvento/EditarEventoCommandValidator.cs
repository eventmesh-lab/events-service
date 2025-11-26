using System;
using FluentValidation;

namespace events_service.Application.Commands.EditarEvento
{
    /// <summary>
    /// Validador para las reglas de negocio de entrada del comando EditarEventoCommand.
    /// Garantiza que la información suministrada cumple los requisitos antes de llegar al dominio.
    /// </summary>
    public class EditarEventoCommandValidator : AbstractValidator<EditarEventoCommand>
    {
        /// <summary>
        /// Inicializa una nueva instancia del validador configurando las reglas necesarias.
        /// </summary>
        public EditarEventoCommandValidator()
        {
            RuleFor(command => command.EventoId)
                .NotEmpty()
                .WithMessage("El identificador del evento es requerido.");

            RuleFor(command => command.Nombre)
                .NotEmpty()
                .WithMessage("El nombre del evento es requerido.");

            RuleFor(command => command.Descripcion)
                .NotNull()
                .WithMessage("La descripción del evento no puede ser nula.");

            RuleFor(command => command.Categoria)
                .NotEmpty()
                .WithMessage("La categoría del evento es requerida.");

            RuleFor(command => command.Fecha)
                .Must(fecha => fecha > DateTime.Now)
                .WithMessage("La fecha del evento debe ser posterior a la fecha actual.");

            RuleFor(command => command.HorasDuracion)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Las horas de duración deben ser mayores o iguales a cero.");

            RuleFor(command => command.MinutosDuracion)
                .InclusiveBetween(0, 59)
                .WithMessage("Los minutos de duración deben estar entre 0 y 59.");

            RuleFor(command => command.Secciones)
                .NotEmpty()
                .WithMessage("Debe existir al menos una sección para el evento.");

            RuleForEach(command => command.Secciones)
                .SetValidator(new SeccionDtoValidator());
        }

        /// <summary>
        /// Validador específico para los DTO de secciones utilizados en la edición del evento.
        /// </summary>
        private class SeccionDtoValidator : AbstractValidator<EditarEventoCommand.SeccionDto>
        {
            /// <summary>
            /// Configura reglas de validación para cada sección enviada en el comando.
            /// </summary>
            public SeccionDtoValidator()
            {
                RuleFor(seccion => seccion.Nombre)
                    .NotEmpty()
                    .WithMessage("El nombre de la sección es requerido.");

                RuleFor(seccion => seccion.Capacidad)
                    .GreaterThan(0)
                    .WithMessage("La capacidad debe ser mayor a cero.");

                RuleFor(seccion => seccion.Precio)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("El precio no puede ser negativo.");
            }
        }
    }
}
