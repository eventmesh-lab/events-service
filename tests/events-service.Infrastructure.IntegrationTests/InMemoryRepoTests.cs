using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using events_service.Domain.Entities;
using events_service.Domain.ValueObjects;
using events_service.Infrastructure.Persistence;
using events_service.Infrastructure.Repositories;

namespace events_service.Infrastructure.IntegrationTests
{
    public class InMemoryRepoTests
    {
        [Fact]
        public async Task AddAndGet_WithInMemoryDatabase_PersistsEvento()
        {
            // Arrange - Configuración optimizada con opciones cacheadas
            var options = new DbContextOptionsBuilder<EventsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging(false) // Desactivar logging innecesario
                .Options;

            await using var context = new EventsDbContext(options);
            var repository = new EventoRepository(context);
            var evento = CrearEventoDePrueba();

            // Act
            await repository.AddAsync(evento);
            var fetched = await repository.GetByIdAsync(evento.Id);

            // Assert
            Assert.NotNull(fetched);
            Assert.Equal(evento.Id, fetched!.Id);
            Assert.Equal(evento.Nombre, fetched.Nombre);
            Assert.Single(fetched.Secciones);
        }

        private static Evento CrearEventoDePrueba()
        {
            var fecha = new FechaEvento(DateTime.UtcNow.AddDays(10));
            var duracion = new DuracionEvento(2, 0);
            var secciones = new[]
            {
                new Seccion("General", 100, new PrecioEntrada(50m))
            };

            return Evento.Crear(
                "Concierto de prueba",
                "Descripción",
                fecha,
                duracion,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Música",
                100m,
                secciones);
        }
    }
}

