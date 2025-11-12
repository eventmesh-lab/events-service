using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace events_service.Infrastructure.Persistence
{
    /// <summary>
    /// Configuración de Entity Framework para la entidad EventoEntity.
    /// Define el mapeo a la tabla eventos en PostgreSQL.
    /// </summary>
    public class EventoEntityConfiguration : IEntityTypeConfiguration<EventoEntity>
    {
        /// <summary>
        /// Configura el mapeo de la entidad EventoEntity.
        /// </summary>
        /// <param name="builder">Constructor de la entidad.</param>
        public void Configure(EntityTypeBuilder<EventoEntity> builder)
        {
            builder.ToTable("eventos");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");

            builder.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.Descripcion)
                .HasColumnName("descripcion")
                .HasColumnType("text");

            builder.Property(e => e.FechaInicio)
                .HasColumnName("fecha_inicio")
                .IsRequired();

            builder.Property(e => e.DuracionHoras)
                .HasColumnName("duracion_horas")
                .IsRequired();

            builder.Property(e => e.DuracionMinutos)
                .HasColumnName("duracion_minutos")
                .IsRequired();

            builder.Property(e => e.Estado)
                .HasColumnName("estado")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .IsRequired();

            builder.Property(e => e.FechaPublicacion)
                .HasColumnName("fecha_publicacion");

            // Relación uno a muchos con Secciones
            builder.HasMany(e => e.Secciones)
                .WithOne(s => s.Evento)
                .HasForeignKey(s => s.EventoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

