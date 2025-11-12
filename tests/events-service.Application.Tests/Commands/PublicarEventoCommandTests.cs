using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
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
    /// Valida la orquestación del caso de uso de publicación de eventos.
    /// </summary>
    public class PublicarEventoCommandTests
    {
        [Fact]
        public void PublicarEventoCommand_ConIdValido_CreaComando()
        {
            // Arrange & Act
            var command = new PublicarEventoCommand
            {
                EventoId = Guid.NewGuid()
            };

            // Assert
            Assert.NotNull(command);
            Assert.NotEqual(Guid.Empty, command.EventoId);
        }

        [Fact]
        public void PublicarEventoCommandValidator_ConIdVacio_RetornaError()
        {
            // Arrange
            var validator = new PublicarEventoCommandValidator();
            var command = new PublicarEventoCommand
            {
                EventoId = Guid.Empty
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(PublicarEventoCommand.EventoId));
        }

        [Fact]
        public async Task PublicarEventoCommandHandler_ConEventoExistenteEnBorrador_PublicaEvento()
        {
            // Arrange
            var eventoId = Guid.NewGuid();
            var evento = CrearEventoEnBorrador(eventoId);

            var mockRepository = new Mock<IEventoRepository>();
            mockRepository.Setup(r => r.GetByIdAsync(eventoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(evento);

            var mockPublisher = new Mock<IPublisher>();
            
            var handler = new PublicarEventoCommandHandler(mockRepository.Object, mockPublisher.Object);
            var command = new PublicarEventoCommand
            {
                EventoId = eventoId
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(evento.Estado.EsPublicado);
            mockRepository.Verify(r => r.UpdateAsync(evento, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PublicarEventoCommandHandler_ConEventoExistenteEnBorrador_PublicaEventoDeDominio()
        {
            // Arrange
            var eventoId = Guid.NewGuid();
            var evento = CrearEventoEnBorrador(eventoId);

            var mockRepository = new Mock<IEventoRepository>();
            mockRepository.Setup(r => r.GetByIdAsync(eventoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(evento);

            var mockPublisher = new Mock<IPublisher>();
            
            var handler = new PublicarEventoCommandHandler(mockRepository.Object, mockPublisher.Object);
            var command = new PublicarEventoCommand
            {
                EventoId = eventoId
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            mockPublisher.Verify(
                p => p.Publish(It.IsAny<EventoPublicado>(), It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task PublicarEventoCommandHandler_ConEventoNoExistente_LanzaExcepcion()
        {
            // Arrange
            var eventoId = Guid.NewGuid();
            var mockRepository = new Mock<IEventoRepository>();
            mockRepository.Setup(r => r.GetByIdAsync(eventoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Evento)null);

            var mockPublisher = new Mock<IPublisher>();
            
            var handler = new PublicarEventoCommandHandler(mockRepository.Object, mockPublisher.Object);
            var command = new PublicarEventoCommand
            {
                EventoId = eventoId
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task PublicarEventoCommandHandler_ConEventoYaPublicado_LanzaExcepcion()
        {
            // Arrange
            var eventoId = Guid.NewGuid();
            var evento = CrearEventoEnBorrador(eventoId);
            evento.Publicar(); // Ya está publicado

            var mockRepository = new Mock<IEventoRepository>();
            mockRepository.Setup(r => r.GetByIdAsync(eventoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(evento);

            var mockPublisher = new Mock<IPublisher>();
            
            var handler = new PublicarEventoCommandHandler(mockRepository.Object, mockPublisher.Object);
            var command = new PublicarEventoCommand
            {
                EventoId = eventoId
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task PublicarEventoCommandHandler_ConEventoCancelado_LanzaExcepcion()
        {
            // Arrange
            var eventoId = Guid.NewGuid();
            var evento = CrearEventoEnBorrador(eventoId);
            evento.Cancelar(); // Está cancelado

            var mockRepository = new Mock<IEventoRepository>();
            mockRepository.Setup(r => r.GetByIdAsync(eventoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(evento);

            var mockPublisher = new Mock<IPublisher>();
            
            var handler = new PublicarEventoCommandHandler(mockRepository.Object, mockPublisher.Object);
            var command = new PublicarEventoCommand
            {
                EventoId = eventoId
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                handler.Handle(command, CancellationToken.None));
        }

        #region Helpers

        private Evento CrearEventoEnBorrador(Guid id)
        {
            var nombre = "Concierto de Rock";
            var descripcion = "Un concierto increíble";
            var fecha = DateTime.UtcNow.AddDays(30);
            var duracion = new DuracionEvento(2, 30);
            var secciones = new System.Collections.Generic.List<Seccion>
            {
                new Seccion("General", 500, new PrecioEntrada(50.00m))
            };

            var evento = Evento.Crear(nombre, descripcion, fecha, duracion, secciones);
            // Nota: En la implementación real, necesitaremos una forma de establecer el ID
            // Por ahora, asumimos que el método Crear acepta un ID opcional o usamos reflexión
            return evento;
        }

        #endregion
    }
}

