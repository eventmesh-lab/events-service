using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events_service.Infrastructure.Persistence
{
    /// <summary>
    /// Configuración de Entity Framework para la entidad SeccionEntity.
    /// Define el mapeo a la tabla secciones en PostgreSQL.
    /// </summary>
    public class SeccionEntityConfiguration : IEntityTypeConfiguration<SeccionEntity>
    {
        /// <summary>
        /// Configura el mapeo de la entidad SeccionEntity.
        /// </summary>
        /// <param name="builder">Constructor de la entidad.</param>
        public void Configure(EntityTypeBuilder<SeccionEntity> builder)
        {
            builder.ToTable("secciones");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            builder.Property(s => s.EventoId)
                .HasColumnName("evento_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(s => s.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(s => s.PrecioMonto)
                .HasColumnName("precio_monto")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(s => s.Capacidad)
                .HasColumnName("capacidad")
                .IsRequired();

            // Índice para mejorar consultas por EventoId
            builder.HasIndex(s => s.EventoId);
        }
    }
}

