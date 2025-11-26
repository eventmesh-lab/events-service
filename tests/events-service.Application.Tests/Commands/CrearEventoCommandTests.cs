using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using Xunit;
using events_service.Application.Commands.CrearEvento;
using events_service.Domain.Entities;
using events_service.Domain.Events;
using events_service.Domain.Ports;
using events_service.Domain.ValueObjects;

namespace events_service.Application.Tests.Commands
{
    /// <summary>
    /// Pruebas para el comando CrearEventoCommand y su handler.
    /// Valida la orquestación del caso de uso de creación de eventos.
    /// </summary>
    public class CrearEventoCommandTests
    {
        // Helper methods para reducir overhead de creación de mocks
        private static Mock<IEventoRepository> CreateRepositoryMock()
        {
            return new Mock<IEventoRepository>();
        }

        private static Mock<IDomainEventPublisher> CreatePublisherMock()
        {
            return new Mock<IDomainEventPublisher>();
        }

        private static Mock<IValidator<CrearEventoCommand>> CreateValidatorMock(bool isValid = true)
        {
            var mock = new Mock<IValidator<CrearEventoCommand>>();
            FluentValidation.Results.ValidationResult result;
            
            if (isValid)
            {
                result = new FluentValidation.Results.ValidationResult();
            }
            else
            {
                result = new FluentValidation.Results.ValidationResult();
                result.Errors.Add(new FluentValidation.Results.ValidationFailure("Nombre", "El nombre es requerido"));
            }
            
            mock.Setup(v => v.ValidateAsync(It.IsAny<CrearEventoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
            return mock;
        }

        // Helper para crear comando válido - reduce duplicación masiva
        private static CrearEventoCommand CrearComandoValido(string nombre = "Concierto de Rock")
        {
            return new CrearEventoCommand
            {
                Nombre = nombre,
                Descripcion = "Un concierto increíble",
                Fecha = DateTime.Now.AddDays(30),
                HorasDuracion = 2,
                MinutosDuracion = 30,
                Secciones = new List<CrearEventoCommand.SeccionDto>
                {
                    new CrearEventoCommand.SeccionDto
                    {
                        Nombre = "General",
                        Capacidad = 500,
                        Precio = 50.00m
                    }
                },
                OrganizadorId = Guid.NewGuid(),
                VenueId = Guid.NewGuid(),
                Categoria = "Música",
                TarifaPublicacion = 100m
            };
        }

        [Fact]
        public void CrearEventoCommand_ConDatosValidos_CreaComando()
        {
            // Arrange & Act
            var command = CrearComandoValido();

            // Assert
            Assert.NotNull(command);
            Assert.Equal("Concierto de Rock", command.Nombre);
        }

        [Fact]
        public void CrearEventoCommandValidator_ConNombreVacio_RetornaError()
        {
            // Arrange
            var validator = new CrearEventoCommandValidator();
            var command = new CrearEventoCommand
            {
                Nombre = string.Empty,
                Descripcion = "Descripción",
                Fecha = DateTime.Now.AddDays(30),
                HorasDuracion = 2,
                MinutosDuracion = 30,
                Secciones = new List<CrearEventoCommand.SeccionDto>()
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CrearEventoCommand.Nombre));
        }

        [Fact]
        public void CrearEventoCommandValidator_ConListaSeccionesVacia_RetornaError()
        {
            // Arrange
            var validator = new CrearEventoCommandValidator();
            var command = new CrearEventoCommand
            {
                Nombre = "Concierto",
                Descripcion = "Descripción",
                Fecha = DateTime.Now.AddDays(30),
                HorasDuracion = 2,
                MinutosDuracion = 30,
                Secciones = new List<CrearEventoCommand.SeccionDto>()
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CrearEventoCommand.Secciones));
        }


        [Fact]
        public void CrearEventoCommandValidator_ConFechaEnElPasado_RetornaError()
        {
            // Arrange
            var validator = new CrearEventoCommandValidator();
            var command = new CrearEventoCommand
            {
                Nombre = "Concierto",
                Descripcion = "Descripción",
                Fecha = DateTime.Now.AddDays(-1), // Fecha inválida
                HorasDuracion = 2,
                MinutosDuracion = 30,
                Secciones = new List<CrearEventoCommand.SeccionDto>
                {
                    new CrearEventoCommand.SeccionDto
                    {
                        Nombre = "General",
                        Capacidad = 500,
                        Precio = 50.00m
                    }
                },
                OrganizadorId = Guid.NewGuid(),
                VenueId = Guid.NewGuid(),
                Categoria = "Música",
                TarifaPublicacion = 100m
            };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CrearEventoCommand.Fecha));
        }


        [Fact]
        public async Task CrearEventoCommandHandler_ConComandoValido_CreaEvento()
        {
            // Arrange
            var mockRepository = CreateRepositoryMock();
            var mockPublisher = CreatePublisherMock();
            var mockValidator = CreateValidatorMock();
            
            var handler = new CrearEventoCommandHandler(mockRepository.Object, mockPublisher.Object, mockValidator.Object);
            var command = CrearComandoValido();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            mockRepository.Verify(r => r.AddAsync(It.IsAny<Evento>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CrearEventoCommandHandler_ConComandoValido_PublicaEventoDeDominio()
        {
            // Arrange
            var mockRepository = CreateRepositoryMock();
            var mockPublisher = CreatePublisherMock();
            var mockValidator = CreateValidatorMock();
            
            var handler = new CrearEventoCommandHandler(mockRepository.Object, mockPublisher.Object, mockValidator.Object);
            var command = CrearComandoValido();

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            mockPublisher.Verify(
                p => p.PublishAsync(It.IsAny<EventoCreado>(), It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task CrearEventoCommandHandler_ConComandoInvalido_LanzaExcepcion()
        {
            // Arrange
            var mockRepository = CreateRepositoryMock();
            var mockPublisher = CreatePublisherMock();
            var mockValidator = CreateValidatorMock(isValid: false);
            
            var handler = new CrearEventoCommandHandler(mockRepository.Object, mockPublisher.Object, mockValidator.Object);
            var command = new CrearEventoCommand
            {
                Nombre = string.Empty, // Inválido
                Descripcion = "Descripción",
                Fecha = DateTime.Now.AddDays(30),
                HorasDuracion = 2,
                MinutosDuracion = 30,
                Secciones = new List<CrearEventoCommand.SeccionDto>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => 
                handler.Handle(command, CancellationToken.None));
        }
    }
}
