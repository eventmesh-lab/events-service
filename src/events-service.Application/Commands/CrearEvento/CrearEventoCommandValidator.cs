using System;
using FluentValidation;

namespace events_service.Application.Commands.CrearEvento
{
    /// <summary>
    /// Validador para el comando CrearEventoCommand.
    /// Valida que todos los datos de entrada sean correctos antes de crear el evento.
    /// </summary>
    public class CrearEventoCommandValidator : AbstractValidator<CrearEventoCommand>
    {
        /// <summary>
        /// Inicializa una nueva instancia del validador con las reglas de validación.
        /// </summary>
        public CrearEventoCommandValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty()
                .WithMessage("El nombre del evento es requerido.");

            RuleFor(x => x.Fecha)
                .NotEmpty()
                .WithMessage("La fecha del evento es requerida.")
                .Must(fecha => fecha > DateTime.Now)
                .WithMessage("La fecha del evento debe ser en el futuro.");

            RuleFor(x => x.HorasDuracion)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Las horas de duración deben ser mayor o igual a cero.");

            RuleFor(x => x.MinutosDuracion)
                .InclusiveBetween(0, 59)
                .WithMessage("Los minutos de duración deben estar entre 0 y 59.");

            RuleFor(x => x.Secciones)
                .NotEmpty()
                .WithMessage("El evento debe tener al menos una sección.");

            RuleForEach(x => x.Secciones)
                .SetValidator(new SeccionDtoValidator());

            RuleFor(x => x.OrganizadorId)
                .NotEmpty()
                .WithMessage("El organizador es requerido.");

            RuleFor(x => x.VenueId)
                .NotEmpty()
                .WithMessage("El venue es requerido.");

            RuleFor(x => x.Categoria)
                .NotEmpty()
                .WithMessage("La categoría es requerida.");

            RuleFor(x => x.TarifaPublicacion)
                .GreaterThanOrEqualTo(0)
                .WithMessage("La tarifa de publicación no puede ser negativa.");
        }
    }

    /// <summary>
    /// Validador para SeccionDto.
    /// </summary>
    public class SeccionDtoValidator : AbstractValidator<CrearEventoCommand.SeccionDto>
    {
        /// <summary>
        /// Inicializa una nueva instancia del validador con las reglas de validación.
        /// </summary>
        public SeccionDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty()
                .WithMessage("El nombre de la sección es requerido.");

            RuleFor(x => x.Capacidad)
                .GreaterThan(0)
                .WithMessage("La capacidad debe ser mayor a cero.");

            RuleFor(x => x.Precio)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El precio no puede ser negativo.");
        }
    }
}

