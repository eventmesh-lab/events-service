using System;
using System.Collections.Generic;
using System.Linq;
using events_service.Domain.Events;
using events_service.Domain.ValueObjects;

namespace events_service.Domain.Entities
{
    /// <summary>
    /// Agregado raíz que representa un evento.
    /// Gestiona el ciclo de vida completo del evento desde su creación hasta su finalización.
    /// </summary>
    public class Evento
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        private readonly List<Seccion> _secciones = new();
        private readonly List<MediaAsset> _imagenesSecundarias = new();

        public Guid Id { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public string Descripcion { get; private set; } = string.Empty;
        public FechaEvento Fecha { get; private set; } = null!;
        public DuracionEvento Duracion { get; private set; } = null!;
        public EstadoEvento Estado { get; private set; } = null!;
        public Guid OrganizadorId { get; private set; }
        public Guid VenueId { get; private set; }
        public string Categoria { get; private set; } = string.Empty;
        public decimal TarifaPublicacion { get; private set; }
        public Guid? TransaccionPagoId { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime? FechaPublicacion { get; private set; }
        public int Version { get; private set; }

        // Propiedades para cancelación
        public string? MotivoCancelacion { get; private set; }
        public DateTime? FechaCancelacion { get; private set; }
        public string? CanceladoPor { get; private set; }

        // Propiedades para reprogramación
        public DateTime? FechaInicioOriginal { get; private set; }
        public DateTime? FechaFinOriginal { get; private set; }
        public int ContadorReprogramaciones { get; private set; }
        public DateTime? UltimaReprogramacionFecha { get; private set; }
        public string? UltimaReprogramacionPor { get; private set; }

        public MediaAsset? ImagenPrincipal { get; private set; }
        public IReadOnlyCollection<MediaAsset> ImagenesSecundarias => _imagenesSecundarias.AsReadOnly();
        public MediaAsset? FolletoPdf { get; private set; }

        public IReadOnlyCollection<Seccion> Secciones => _secciones.AsReadOnly();

        private Evento()
        {
        }

        public static Evento Crear(
            string nombre,
            string descripcion,
            FechaEvento fecha,
            DuracionEvento duracion,
            Guid organizadorId,
            Guid venueId,
            string categoria,
            decimal tarifaPublicacion,
            ICollection<Seccion> secciones)
        {
            if (nombre is null)
            {
                throw new ArgumentNullException(nameof(nombre), "El nombre no puede ser nulo.");
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
            }

            if (duracion is null)
            {
                throw new ArgumentNullException(nameof(duracion));
            }

            if (fecha is null)
            {
                throw new ArgumentNullException(nameof(fecha));
            }

            if (organizadorId == Guid.Empty)
            {
                throw new ArgumentException("El organizador es requerido.", nameof(organizadorId));
            }

            if (venueId == Guid.Empty)
            {
                throw new ArgumentException("El venue es requerido.", nameof(venueId));
            }

            if (string.IsNullOrWhiteSpace(categoria))
            {
                throw new ArgumentException("La categoría es requerida.", nameof(categoria));
            }

            if (tarifaPublicacion < 0)
            {
                throw new ArgumentException("La tarifa de publicación no puede ser negativa.", nameof(tarifaPublicacion));
            }

            if (secciones is null)
            {
                throw new ArgumentNullException(nameof(secciones), "Las secciones no pueden ser nulas.");
            }

            if (secciones.Count == 0)
            {
                throw new ArgumentException("El evento debe tener al menos una sección.", nameof(secciones));
            }

            var ahora = DateTime.UtcNow;

            var evento = new Evento
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = descripcion ?? string.Empty,
                Fecha = fecha,
                Duracion = duracion,
                Estado = new EstadoEvento("Borrador"),
                OrganizadorId = organizadorId,
                VenueId = venueId,
                Categoria = categoria,
                TarifaPublicacion = Math.Round(tarifaPublicacion, 2, MidpointRounding.AwayFromZero),
                FechaCreacion = ahora,
                Version = 1
            };

            evento.ReemplazarSecciones(secciones);

            evento.RegistrarEvento(new EventoCreado(evento.Id, evento.Nombre, evento.OrganizadorId, evento.Fecha.Valor, evento.FechaCreacion));

            return evento;
        }

        public void Editar(
            string nombre,
            string descripcion,
            FechaEvento fecha,
            DuracionEvento duracion,
            string categoria,
            IEnumerable<Seccion> secciones)
        {
            AsegurarEstado(Estado.EsBorrador, "Solo se pueden editar eventos en estado Borrador.");

            if (nombre is null)
            {
                throw new ArgumentNullException(nameof(nombre));
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
            }

            if (fecha is null)
            {
                throw new ArgumentNullException(nameof(fecha));
            }

            if (duracion is null)
            {
                throw new ArgumentNullException(nameof(duracion));
            }

            if (string.IsNullOrWhiteSpace(categoria))
            {
                throw new ArgumentException("La categoría es requerida.", nameof(categoria));
            }

            if (secciones is null)
            {
                throw new ArgumentNullException(nameof(secciones));
            }

            var listaSecciones = secciones.ToList();

            if (listaSecciones.Count == 0)
            {
                throw new ArgumentException("El evento debe tener al menos una sección.", nameof(secciones));
            }

            Nombre = nombre;
            Descripcion = descripcion ?? string.Empty;
            Fecha = fecha;
            Duracion = duracion;
            Categoria = categoria;

            ReemplazarSecciones(listaSecciones);
            IncrementarVersion();

            RegistrarEvento(new EventoEditado(Id, DateTime.UtcNow));
        }

        public void AgregarSeccion(Seccion seccion)
        {
            AsegurarEstado(Estado.EsBorrador, "Solo se pueden modificar secciones en estado Borrador.");

            if (seccion == null)
            {
                throw new ArgumentNullException(nameof(seccion), "La sección no puede ser nula.");
            }

            if (_secciones.Any(s => s.Id == seccion.Id || s.Nombre.Equals(seccion.Nombre, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("No se puede agregar una sección duplicada.");
            }

            _secciones.Add(seccion);
            IncrementarVersion();
        }

        public void PagarPublicacion(Guid transaccionPagoId, decimal monto)
        {
            AsegurarEstado(Estado.EsBorrador, "Solo se puede iniciar el pago cuando el evento está en Borrador.");

            if (transaccionPagoId == Guid.Empty)
            {
                throw new ArgumentException("La transacción es requerida.", nameof(transaccionPagoId));
            }

            if (monto <= 0)
            {
                throw new ArgumentException("El monto debe ser mayor a cero.", nameof(monto));
            }

            if (Math.Round(monto, 2, MidpointRounding.AwayFromZero) != TarifaPublicacion)
            {
                throw new InvalidOperationException("El monto del pago no coincide con la tarifa de publicación configurada.");
            }

            TransaccionPagoId = transaccionPagoId;
            Estado = new EstadoEvento("PendientePago");
            IncrementarVersion();

            RegistrarEvento(new PagoPublicacionIniciado(Id, transaccionPagoId, monto));
        }

        public void Publicar(Guid pagoConfirmadoId, DateTime fechaActual)
        {
            AsegurarEstado(Estado.EsPendientePago, "Solo se pueden publicar eventos con pago pendiente.");

            if (pagoConfirmadoId == Guid.Empty)
            {
                throw new ArgumentException("El identificador de pago es requerido.", nameof(pagoConfirmadoId));
            }

            if (TransaccionPagoId != pagoConfirmadoId)
            {
                throw new InvalidOperationException("El pago confirmado no corresponde con la transacción registrada.");
            }

            Estado = new EstadoEvento("Publicado");
            FechaPublicacion = fechaActual;
            TransaccionPagoId = null;
            IncrementarVersion();

            RegistrarEvento(new EventoPublicado(Id, Nombre, OrganizadorId, FechaPublicacion.Value));
        }

        public void Iniciar(DateTime fechaActual)
        {
            AsegurarEstado(Estado.EsPublicado, "Solo se pueden iniciar eventos publicados.");

            if (!Fecha.HaComenzado(fechaActual))
            {
                throw new InvalidOperationException("No se puede iniciar el evento antes de la fecha programada.");
            }

            Estado = new EstadoEvento("EnCurso");
            IncrementarVersion();

            RegistrarEvento(new EventoIniciado(Id, fechaActual));
        }

        public void Finalizar(DateTime fechaActual)
        {
            AsegurarEstado(Estado.EsEnCurso, "Solo se pueden finalizar eventos en curso.");

            Estado = new EstadoEvento("Finalizado");
            IncrementarVersion();

            RegistrarEvento(new EventoFinalizado(Id, fechaActual));
        }

        public void Cancelar(string motivo, string canceladoPor, DateTime fechaActual)
        {
            if (Estado.EsFinalizado)
            {
                throw new InvalidOperationException("No se puede cancelar un evento finalizado.");
            }

            if (Estado.EsEnCurso)
            {
                throw new InvalidOperationException("No se puede cancelar un evento en curso.");
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                throw new ArgumentException("El motivo de cancelación es requerido.", nameof(motivo));
            }

            if (string.IsNullOrWhiteSpace(canceladoPor))
            {
                throw new ArgumentException("El usuario que cancela es requerido.", nameof(canceladoPor));
            }

            Estado = new EstadoEvento("Cancelado");
            MotivoCancelacion = motivo;
            FechaCancelacion = fechaActual;
            CanceladoPor = canceladoPor;
            TransaccionPagoId = null;
            IncrementarVersion();

            RegistrarEvento(new EventoCancelado(Id, motivo));
        }

        public void Reprogramar(FechaEvento nuevaFecha, DuracionEvento nuevaDuracion, string reprogramadoPor, DateTime fechaActual)
        {
            if (!Estado.EsPublicado)
            {
                throw new InvalidOperationException("Solo eventos publicados pueden ser reprogramados.");
            }

            if (nuevaFecha is null)
            {
                throw new ArgumentNullException(nameof(nuevaFecha));
            }

            if (nuevaDuracion is null)
            {
                throw new ArgumentNullException(nameof(nuevaDuracion));
            }

            if (string.IsNullOrWhiteSpace(reprogramadoPor))
            {
                throw new ArgumentException("El usuario que reprograma es requerido.", nameof(reprogramadoPor));
            }

            if (nuevaFecha.Valor <= fechaActual)
            {
                throw new InvalidOperationException("La nueva fecha debe ser futura.");
            }

            // Respaldar fechas originales si es la primera vez
            if (ContadorReprogramaciones == 0)
            {
                FechaInicioOriginal = Fecha.Valor;
                // Asumiendo que podemos calcular la fecha fin basada en duración
                FechaFinOriginal = Fecha.Valor.AddHours(Duracion.Horas).AddMinutes(Duracion.Minutos);
            }

            Fecha = nuevaFecha;
            Duracion = nuevaDuracion;
            ContadorReprogramaciones++;
            UltimaReprogramacionFecha = fechaActual;
            UltimaReprogramacionPor = reprogramadoPor;
            
            IncrementarVersion();

            RegistrarEvento(new EventoReprogramado(Id, Fecha.Valor, reprogramadoPor, fechaActual));
        }

        /// <summary>
        /// Asigna la imagen principal del evento (validando que sea imagen válida <=1MB).
        /// </summary>
        public void DefinirImagenPrincipal(MediaAsset imagen)
        {
            if (imagen == null)
            {
                throw new ArgumentNullException(nameof(imagen));
            }

            if (!imagen.EsImagen)
            {
                throw new InvalidOperationException("La imagen principal debe ser un archivo de imagen soportado.");
            }

            ImagenPrincipal = imagen;
            IncrementarVersion();
        }

        /// <summary>
        /// Reemplaza el conjunto de imágenes secundarias (máximo 5, todas deben ser imágenes válidas).
        /// </summary>
        public void DefinirImagenesSecundarias(IEnumerable<MediaAsset> imagenes)
        {
            if (imagenes == null)
            {
                throw new ArgumentNullException(nameof(imagenes));
            }

            var lista = imagenes.ToList();

            if (lista.Count > 5)
            {
                throw new InvalidOperationException("No se pueden asociar más de 5 imágenes secundarias.");
            }

            if (lista.Any(img => img == null || !img.EsImagen))
            {
                throw new InvalidOperationException("Todas las imágenes secundarias deben ser archivos de imagen soportados.");
            }

            _imagenesSecundarias.Clear();
            _imagenesSecundarias.AddRange(lista);
            IncrementarVersion();
        }

        /// <summary>
        /// Asigna el folleto en PDF (validando tipo y tamaño).
        /// </summary>
        public void DefinirFolleto(MediaAsset folleto)
        {
            if (folleto == null)
            {
                throw new ArgumentNullException(nameof(folleto));
            }

            if (!folleto.EsPdf)
            {
                throw new InvalidOperationException("El folleto debe ser un archivo PDF válido.");
            }

            FolletoPdf = folleto;
            IncrementarVersion();
        }

        /// <summary>
        /// Sincroniza los blobs desde la capa de persistencia sin disparar reglas adicionales.
        /// </summary>
        public void SincronizarMedia(MediaAsset? imagenPrincipal, IEnumerable<MediaAsset>? imagenesSecundarias, MediaAsset? folleto)
        {
            ImagenPrincipal = imagenPrincipal;

            _imagenesSecundarias.Clear();
            if (imagenesSecundarias != null)
            {
                _imagenesSecundarias.AddRange(imagenesSecundarias);
            }

            FolletoPdf = folleto;
        }

        public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

        public void ClearDomainEvents() => _domainEvents.Clear();

        private void RegistrarEvento(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        private void AsegurarEstado(bool condicion, string mensaje)
        {
            if (!condicion)
            {
                throw new InvalidOperationException(mensaje);
            }
        }

        private void ReemplazarSecciones(IEnumerable<Seccion> secciones)
        {
            _secciones.Clear();
            _secciones.AddRange(secciones);
        }

        private void IncrementarVersion()
        {
            Version++;
        }
    }
}

