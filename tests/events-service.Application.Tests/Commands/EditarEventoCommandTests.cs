#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using Xunit;
using events_service.Application.Commands.EditarEvento;
using events_service.Domain.Entities;
using events_service.Domain.Events;
using events_service.Domain.Ports;
using events_service.Domain.ValueObjects;

namespace events_service.Application.Tests.Commands
{
    /// <summary>
    /// Pruebas para el comando EditarEventoCommand y su handler.
    /// </summary>
    public class EditarEventoCommandTests
    {
        // Helper methods para reducir overhead de creación de mocks
        private static Mock<IEventoRepository> CreateRepositoryMock(Evento? evento = null)
        {
            var mock = new Mock<IEventoRepository>();
            if (evento != null)
            {
                mock.Setup(r => r.GetByIdAsync(evento.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(evento);
            }
            return mock;
        }

        private static Mock<IDomainEventPublisher> CreatePublisherMock()
        {
            return new Mock<IDomainEventPublisher>();
        }

        private static Mock<IValidator<EditarEventoCommand>> CreateValidatorMock(bool isValid = true)
        {
            var mock = new Mock<IValidator<EditarEventoCommand>>();
            FluentValidation.Results.ValidationResult result;
            
            if (isValid)
            {
                result = new FluentValidation.Results.ValidationResult();
            }
            else
            {
                result = new FluentValidation.Results.ValidationResult
                {
                    Errors = { new FluentValidation.Results.ValidationFailure("Nombre", "Requerido") }
                };
            }
            
            mock.Setup(v => v.ValidateAsync(It.IsAny<EditarEventoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
            return mock;
        }

        [Fact]
        public void EditarEventoCommandValidator_ConEventoIdVacio_FallaValidacion()
        {
            // Arrange
            var validator = new EditarEventoCommandValidator();
            var command = CrearComandoValido();
            command = command with { EventoId = Guid.Empty };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task EditarEventoCommandHandler_ConDatosValidos_ActualizaEvento()
        {
            // Arrange
            var evento = CrearEventoBorrador();
            var command = CrearComandoValido(evento);

            var repositoryMock = CreateRepositoryMock(evento);
            var publisherMock = CreatePublisherMock();
            var validatorMock = CreateValidatorMock();

            var handler = new EditarEventoCommandHandler(repositoryMock.Object, publisherMock.Object, validatorMock.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(command.Nombre, evento.Nombre);
            Assert.Equal(command.Categoria, evento.Categoria);
            repositoryMock.Verify(r => r.UpdateAsync(evento, It.IsAny<CancellationToken>()), Times.Once);
            publisherMock.Verify(p => p.PublishAsync(It.IsAny<EventoEditado>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task EditarEventoCommandHandler_CuandoEventoNoExiste_LanzaExcepcion()
        {
            // Arrange
            var command = CrearComandoValido();

            var repositoryMock = new Mock<IEventoRepository>();
            repositoryMock.Setup(r => r.GetByIdAsync(command.EventoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Evento?)null);

            var publisherMock = CreatePublisherMock();
            var validatorMock = CreateValidatorMock();

            var handler = new EditarEventoCommandHandler(repositoryMock.Object, publisherMock.Object, validatorMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task EditarEventoCommandHandler_ConValidacionFallida_LanzaValidationException()
        {
            // Arrange
            var evento = CrearEventoBorrador();
            var command = CrearComandoValido(evento);

            var repositoryMock = CreateRepositoryMock(evento);
            var publisherMock = CreatePublisherMock();
            var validatorMock = CreateValidatorMock(isValid: false);

            var handler = new EditarEventoCommandHandler(repositoryMock.Object, publisherMock.Object, validatorMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
        }

        private static EditarEventoCommand CrearComandoValido(Evento? evento = null)
        {
            evento ??= CrearEventoBorrador();
            var seccion = evento.Secciones.First();

            return new EditarEventoCommand
            {
                EventoId = evento.Id,
                Nombre = "Evento Editado",
                Descripcion = "Descripción actualizada",
                Fecha = DateTime.UtcNow.AddDays(10),
                HorasDuracion = evento.Duracion.Horas,
                MinutosDuracion = evento.Duracion.Minutos,
                Categoria = "Categoría",
                Secciones = new List<EditarEventoCommand.SeccionDto>
                {
                    new()
                    {
                        Id = seccion.Id,
                        Nombre = seccion.Nombre,
                        Capacidad = seccion.Capacidad + 10,
                        Precio = seccion.Precio.Valor
                    }
                }
            };
        }

        private static Evento CrearEventoBorrador()
        {
            var secciones = new List<Seccion>
            {
                new("General", 100, new PrecioEntrada(50))
            };

            return Evento.Crear(
                "Evento Demo",
                "Descripción",
                new FechaEvento(DateTime.UtcNow.AddDays(5)),
                new DuracionEvento(2, 0),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Música",
                100m,
                secciones);
        }
    }
}
