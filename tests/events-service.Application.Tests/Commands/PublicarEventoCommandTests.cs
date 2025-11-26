#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using Xunit;
using events_service.Application.Commands.PublicarEvento;
using events_service.Domain.Entities;
using events_service.Domain.Events;
using events_service.Domain.Ports;
using events_service.Domain.ValueObjects;

namespace events_service.Application.Tests.Commands
{
    /// <summary>
    /// Pruebas para el comando PublicarEventoCommand y su handler.
    /// </summary>
    public class PublicarEventoCommandTests
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

        private static Mock<IValidator<PublicarEventoCommand>> CreateValidatorMock(bool isValid = true)
        {
            var mock = new Mock<IValidator<PublicarEventoCommand>>();
            var result = isValid 
                ? new FluentValidation.Results.ValidationResult() 
                : new FluentValidation.Results.ValidationResult(new[] { new FluentValidation.Results.ValidationFailure("Test", "Error") });
            
            mock.Setup(v => v.ValidateAsync(It.IsAny<PublicarEventoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
            return mock;
        }

        [Fact]
        public void PublicarEventoCommandValidator_ConPagoConfirmadoVacio_FallaValidacion()
        {
            // Arrange
            var validator = new PublicarEventoCommandValidator();
            var command = new PublicarEventoCommand
            {
                EventoId = Guid.NewGuid(),
                PagoConfirmadoId = Guid.Empty
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task PublicarEventoCommandHandler_ConEventoPendientePago_PublicaEvento()
        {
            // Arrange
            var evento = CrearEventoPendienteDePublicacion();
            var command = new PublicarEventoCommand
            {
                EventoId = evento.Id,
                PagoConfirmadoId = evento.TransaccionPagoId!.Value
            };

            var repositoryMock = CreateRepositoryMock(evento);
            var publisherMock = CreatePublisherMock();
            var validatorMock = CreateValidatorMock();

            var handler = new PublicarEventoCommandHandler(repositoryMock.Object, publisherMock.Object, validatorMock.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(evento.Estado.EsPublicado);
            repositoryMock.Verify(r => r.UpdateAsync(evento, It.IsAny<CancellationToken>()), Times.Once);
            publisherMock.Verify(p => p.PublishAsync(It.IsAny<EventoPublicado>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PublicarEventoCommandHandler_CuandoEventoNoExiste_LanzaExcepcion()
        {
            // Arrange
            var command = new PublicarEventoCommand
            {
                EventoId = Guid.NewGuid(),
                PagoConfirmadoId = Guid.NewGuid()
            };

            var repositoryMock = new Mock<IEventoRepository>();
            repositoryMock.Setup(r => r.GetByIdAsync(command.EventoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Evento?)null);

            var publisherMock = CreatePublisherMock();
            var validatorMock = CreateValidatorMock();

            var handler = new PublicarEventoCommandHandler(repositoryMock.Object, publisherMock.Object, validatorMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        }

        private static Evento CrearEventoPendienteDePublicacion()
        {
            var secciones = new[]
            {
                new Seccion("General", 100, new PrecioEntrada(50m))
            };

            var evento = Evento.Crear(
                "Evento",
                "Descripción",
                new FechaEvento(DateTime.UtcNow.AddDays(3)),
                new DuracionEvento(2, 0),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Música",
                100m,
                secciones);

            var transaccionId = Guid.NewGuid();
            evento.PagarPublicacion(transaccionId, evento.TarifaPublicacion);
            return evento;
        }
    }
}
