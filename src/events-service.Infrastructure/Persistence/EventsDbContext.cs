using Microsoft.EntityFrameworkCore;

namespace events_service.Infrastructure.Persistence
{
    /// <summary>
    /// Contexto de base de datos para Entity Framework Core.
    /// Gestiona la conexión y configuración de la base de datos PostgreSQL.
    /// </summary>
    public class EventsDbContext : DbContext
    {
        /// <summary>
        /// Inicializa una nueva instancia del contexto.
        /// </summary>
        /// <param name="options">Opciones de configuración del contexto.</param>
        public EventsDbContext(DbContextOptions<EventsDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Conjunto de entidades de eventos.
        /// </summary>
        public DbSet<EventoEntity> Eventos { get; set; } = null!;

        /// <summary>
        /// Conjunto de entidades de secciones.
        /// </summary>
        public DbSet<SeccionEntity> Secciones { get; set; } = null!;

        /// <summary>
        /// Configura el modelo de datos usando Fluent API.
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar configuraciones
            modelBuilder.ApplyConfiguration(new EventoEntityConfiguration());
            modelBuilder.ApplyConfiguration(new SeccionEntityConfiguration());
        }
    }
}

