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
    /// <summary>
    /// Colección de tests para aislar la base de datos InMemory y permitir paralelización segura.
    /// </summary>
    [Collection("InMemoryDatabase")]
    public class InMemoryRepoTests
    {
        [Fact]
        public async Task AddAndGet_WithInMemoryDatabase_PersistsEvento()
        {
            // Arrange - Configuración optimizada: nombre único por test para evitar conflictos
            var databaseName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<EventsDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .EnableSensitiveDataLogging(false) // Desactivar logging innecesario
                .EnableServiceProviderCaching(false) // Evitar cache de servicios para mejor aislamiento
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
            // Usar DateTime.UtcNow una sola vez para mejor rendimiento
            var fechaBase = DateTime.UtcNow;
            var fecha = new FechaEvento(fechaBase.AddDays(10));
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

    /// <summary>
    /// Collection definition para agrupar tests que usan InMemory DB.
    /// Esto permite paralelización segura entre diferentes collections.
    /// </summary>
    [CollectionDefinition("InMemoryDatabase")]
    public class InMemoryDatabaseCollection : ICollectionFixture<InMemoryDatabaseFixture>
    {
        // Esta clase solo se usa para definir la collection, no necesita código
    }

    /// <summary>
    /// Fixture compartido para optimizar la creación de configuración de DbContext.
    /// </summary>
    public class InMemoryDatabaseFixture : IDisposable
    {
        public InMemoryDatabaseFixture()
        {
            // Inicialización compartida si es necesaria
        }

        public void Dispose()
        {
            // Limpieza si es necesaria
        }
    }
}


