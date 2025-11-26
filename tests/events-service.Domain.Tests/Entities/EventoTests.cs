using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using events_service.Domain.Entities;
using events_service.Domain.ValueObjects;
using events_service.Domain.Events;

namespace events_service.Domain.Tests.Entities
{
    /// <summary>
    /// Pruebas para el agregado Evento.
    /// Valida las invariantes y comportamiento del aggregate root que gestiona el ciclo de vida de eventos.
    /// </summary>
    public class EventoTests
    {
        #region Creación

        [Fact]
        public void Crear_ConParametrosValidos_CreaEventoEnBorrador()
        {
            // Act
            var evento = CrearEventoBorrador();

            // Assert
            Assert.NotNull(evento);
            Assert.True(evento.Estado.EsBorrador);
            Assert.Equal(Categoria, evento.Categoria);
            Assert.Equal(Tarifa, evento.TarifaPublicacion);
            Assert.NotEqual(Guid.Empty, evento.Id);
            Assert.Equal(1, evento.Version);

            var domainEvents = evento.GetDomainEvents();
            Assert.Single(domainEvents);
            Assert.IsType<EventoCreado>(domainEvents.First());
        }

        [Fact]
        public void Crear_SinSecciones_LanzaExcepcion()
        {
            // Arrange
            var fecha = new FechaEvento(DateTime.Now.AddDays(1));

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                Evento.Crear(NombreEvento, Descripcion, fecha, Duracion, OrganizadorId, VenueId, Categoria, Tarifa, new List<Seccion>()));
        }

        [Fact]
        public void Editar_EnBorrador_ActualizaDatosYGeneraEvento()
        {
            // Arrange
            var evento = CrearEventoBorrador();
            evento.ClearDomainEvents();
            var fechaActualizada = new FechaEvento(DateTime.Now.AddDays(5));
            var nuevasSecciones = new[]
            {
                new Seccion("VIP", 50, new PrecioEntrada(200m))
            };

            // Act
            evento.Editar("Evento Actualizado", "Nueva descripción", fechaActualizada, new DuracionEvento(1, 0), "Música", nuevasSecciones);

            // Assert
            Assert.Equal("Evento Actualizado", evento.Nombre);
            Assert.Equal("Nueva descripción", evento.Descripcion);
            Assert.Equal("Música", evento.Categoria);
            Assert.Single(evento.Secciones);
            Assert.Equal(2, evento.Version);

            var domainEvents = evento.GetDomainEvents();
            Assert.Single(domainEvents);
            Assert.IsType<EventoEditado>(domainEvents.First());
        }

        [Fact]
        public void Editar_FueraDeBorrador_LanzaExcepcion()
        {
            // Arrange
            var evento = CrearEventoBorrador();
            evento.PagarPublicacion(Guid.NewGuid(), Tarifa);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                evento.Editar(NombreEvento, Descripcion, evento.Fecha, Duracion, Categoria, evento.Secciones));
        }

        [Fact]
        public void PagarPublicacion_ConMontoCorrecto_MarcaPendientePago()
        {
            // Arrange
            var evento = CrearEventoBorrador();
            evento.ClearDomainEvents();
            var transaccion = Guid.NewGuid();

            // Act
            evento.PagarPublicacion(transaccion, Tarifa);

            // Assert
            Assert.True(evento.Estado.EsPendientePago);
            Assert.Equal(transaccion, evento.TransaccionPagoId);
            Assert.Equal(2, evento.Version);

            var domainEvents = evento.GetDomainEvents();
            Assert.Single(domainEvents);
            var pagoEvento = Assert.IsType<PagoPublicacionIniciado>(domainEvents.First());
            Assert.Equal(transaccion, pagoEvento.TransaccionPagoId);
            Assert.Equal(Tarifa, pagoEvento.Monto);
        }

        [Fact]
        public void PagarPublicacion_ConMontoIncorrecto_LanzaExcepcion()
        {
            // Arrange
            var evento = CrearEventoBorrador();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => evento.PagarPublicacion(Guid.NewGuid(), Tarifa + 10));
        }

        [Fact]
        public void Publicar_ConPagoConfirmado_CambiaAPublicadoYEmiteEvento()
        {
            // Arrange
            var evento = CrearEventoBorrador();
            var transaccion = Guid.NewGuid();
            evento.PagarPublicacion(transaccion, Tarifa);
            evento.ClearDomainEvents();
            var fechaPublicacion = DateTime.Now;

            // Act
            evento.Publicar(transaccion, fechaPublicacion);

            // Assert
            Assert.True(evento.Estado.EsPublicado);
            Assert.Null(evento.TransaccionPagoId);
            Assert.Equal(fechaPublicacion, evento.FechaPublicacion);

            var domainEvents = evento.GetDomainEvents();
            Assert.Single(domainEvents);
            var publicado = Assert.IsType<EventoPublicado>(domainEvents.First());
            Assert.Equal(evento.Id, publicado.EventoId);
            Assert.Equal(evento.OrganizadorId, publicado.OrganizadorId);
        }

        [Fact]
        public void Publicar_ConTransaccionDiferente_LanzaExcepcion()
        {
            // Arrange
            var evento = CrearEventoBorrador();
            var transaccion = Guid.NewGuid();
            evento.PagarPublicacion(transaccion, Tarifa);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => evento.Publicar(Guid.NewGuid(), DateTime.Now));
        }

        [Fact]
        public void Iniciar_ConFechaAlcanzada_CambiaAEnCurso()
        {
            // Arrange
            var evento = CrearEventoBorrador(fecha: DateTime.Now.AddMinutes(1));
            var transaccion = Guid.NewGuid();
            evento.PagarPublicacion(transaccion, Tarifa);
            var fechaPublicacion = DateTime.Now;
            evento.Publicar(transaccion, fechaPublicacion);
            evento.ClearDomainEvents();

            var fechaInicio = evento.Fecha.Valor.AddMinutes(1);

            // Act
            evento.Iniciar(fechaInicio);

            // Assert
            Assert.True(evento.Estado.EsEnCurso);
            var domainEvents = evento.GetDomainEvents();
            Assert.Single(domainEvents);
            Assert.IsType<EventoIniciado>(domainEvents.First());
        }

        [Fact]
        public void Finalizar_DesdeEnCurso_CambiaAFinalizado()
        {
            // Arrange
            var evento = CrearEventoBorrador(fecha: DateTime.Now.AddMinutes(1));
            var transaccion = Guid.NewGuid();
            evento.PagarPublicacion(transaccion, Tarifa);
            evento.Publicar(transaccion, DateTime.Now);
            evento.Iniciar(evento.Fecha.Valor);
            evento.ClearDomainEvents();

            var fechaFinalizacion = DateTime.Now.AddMinutes(5);

            // Act
            evento.Finalizar(fechaFinalizacion);

            // Assert
            Assert.True(evento.Estado.EsFinalizado);
            var domainEvents = evento.GetDomainEvents();
            Assert.Single(domainEvents);
            Assert.IsType<EventoFinalizado>(domainEvents.First());
        }

        [Fact]
        public void Cancelar_DesdePublicado_CambiaACanceladoYEmiteEvento()
        {
            // Arrange
            var evento = CrearEventoBorrador();
            var transaccion = Guid.NewGuid();
            evento.PagarPublicacion(transaccion, Tarifa);
            evento.Publicar(transaccion, DateTime.Now);
            evento.ClearDomainEvents();

            // Act
            evento.Cancelar("Motivo");

            // Assert
            Assert.True(evento.Estado.EsCancelado);
            var domainEvents = evento.GetDomainEvents();
            Assert.Single(domainEvents);
            Assert.IsType<EventoCancelado>(domainEvents.First());
        }

        [Fact]
        public void Cancelar_EnCurso_LanzaExcepcion()
        {
            // Arrange
            var evento = CrearEventoBorrador(fecha: DateTime.Now.AddMinutes(1));
            var transaccion = Guid.NewGuid();
            evento.PagarPublicacion(transaccion, Tarifa);
            evento.Publicar(transaccion, DateTime.Now);
            evento.Iniciar(evento.Fecha.Valor);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => evento.Cancelar("Motivo"));
        }

        #endregion

        #region Helpers

        private static readonly Guid OrganizadorId = Guid.NewGuid();
        private static readonly Guid VenueId = Guid.NewGuid();
        private const string NombreEvento = "Concierto de Rock";
        private const string Descripcion = "Evento de prueba";
        private const string Categoria = "Música";
        private const decimal Tarifa = 100m;
        private static readonly DuracionEvento Duracion = new(2, 0);

        private Evento CrearEventoBorrador(DateTime? fecha = null)
        {
            var fechaEvento = new FechaEvento(fecha ?? DateTime.Now.AddDays(10));
            var secciones = new List<Seccion>
            {
                new Seccion("General", 500, new PrecioEntrada(50.00m))
            };

            return Evento.Crear(NombreEvento, Descripcion, fechaEvento, Duracion, OrganizadorId, VenueId, Categoria, Tarifa, secciones);
        }

        #endregion
    }
}

