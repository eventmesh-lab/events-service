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
        public void Crear_ConParametrosValidos_CreaEventoEnEstadoBorrador()
        {
            // Arrange
            var nombre = "Concierto de Rock";
            var descripcion = "Un concierto increíble";
            var fecha = DateTime.UtcNow.AddDays(30);
            var duracion = new DuracionEvento(2, 30);
            var secciones = new List<Seccion>
            {
                new Seccion("General", 500, new PrecioEntrada(50.00m))
            };

            // Act
            var evento = Evento.Crear(nombre, descripcion, fecha, duracion, secciones);

            // Assert
            Assert.NotNull(evento);
            Assert.Equal(nombre, evento.Nombre);
            Assert.Equal(descripcion, evento.Descripcion);
            Assert.True(evento.Estado.EsBorrador);
            Assert.NotEqual(Guid.Empty, evento.Id);
            Assert.Single(evento.Secciones);
        }

        [Fact]
        public void Crear_ConNombreNulo_LanzaExcepcion()
        {
            // Arrange
            var descripcion = "Descripción";
            var fecha = DateTime.UtcNow.AddDays(30);
            var duracion = new DuracionEvento(2, 30);
            var secciones = new List<Seccion>
            {
                new Seccion("General", 500, new PrecioEntrada(50.00m))
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                Evento.Crear(null, descripcion, fecha, duracion, secciones));
        }

        [Fact]
        public void Crear_ConNombreVacio_LanzaExcepcion()
        {
            // Arrange
            var descripcion = "Descripción";
            var fecha = DateTime.UtcNow.AddDays(30);
            var duracion = new DuracionEvento(2, 30);
            var secciones = new List<Seccion>
            {
                new Seccion("General", 500, new PrecioEntrada(50.00m))
            };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => 
                Evento.Crear(string.Empty, descripcion, fecha, duracion, secciones));
            Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Crear_ConListaSeccionesVacia_LanzaExcepcion()
        {
            // Arrange
            var nombre = "Concierto";
            var descripcion = "Descripción";
            var fecha = DateTime.UtcNow.AddDays(30);
            var duracion = new DuracionEvento(2, 30);
            var secciones = new List<Seccion>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => 
                Evento.Crear(nombre, descripcion, fecha, duracion, secciones));
            Assert.Contains("sección", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Crear_ConSeccionesNulas_LanzaExcepcion()
        {
            // Arrange
            var nombre = "Concierto";
            var descripcion = "Descripción";
            var fecha = DateTime.UtcNow.AddDays(30);
            var duracion = new DuracionEvento(2, 30);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                Evento.Crear(nombre, descripcion, fecha, duracion, null));
        }

        [Fact]
        public void Crear_GeneraEventoDeDominioEventoCreado()
        {
            // Arrange
            var nombre = "Concierto";
            var descripcion = "Descripción";
            var fecha = DateTime.UtcNow.AddDays(30);
            var duracion = new DuracionEvento(2, 30);
            var secciones = new List<Seccion>
            {
                new Seccion("General", 500, new PrecioEntrada(50.00m))
            };

            // Act
            var evento = Evento.Crear(nombre, descripcion, fecha, duracion, secciones);

            // Assert
            var eventos = evento.GetDomainEvents();
            Assert.Single(eventos);
            Assert.IsType<EventoCreado>(eventos.First());
        }

        #endregion

        #region Publicación

        [Fact]
        public void Publicar_ConEventoEnBorrador_CambiaEstadoAPublicado()
        {
            // Arrange
            var evento = CrearEventoEnBorrador();

            // Act
            evento.Publicar();

            // Assert
            Assert.True(evento.Estado.EsPublicado);
        }

        [Fact]
        public void Publicar_ConEventoEnBorrador_GeneraEventoDeDominioEventoPublicado()
        {
            // Arrange
            var evento = CrearEventoEnBorrador();
            evento.ClearDomainEvents(); // Limpiar eventos previos

            // Act
            evento.Publicar();

            // Assert
            var eventos = evento.GetDomainEvents();
            Assert.Single(eventos);
            Assert.IsType<EventoPublicado>(eventos.First());
        }

        [Fact]
        public void Publicar_ConEventoYaPublicado_LanzaExcepcion()
        {
            // Arrange
            var evento = CrearEventoEnBorrador();
            evento.Publicar(); // Primera publicación

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => evento.Publicar());
            Assert.Contains("estado", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Publicar_ConEventoFinalizado_LanzaExcepcion()
        {
            // Arrange
            var evento = CrearEventoEnBorrador();
            evento.Publicar();
            evento.Finalizar();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => evento.Publicar());
            Assert.Contains("estado", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Publicar_ConEventoCancelado_LanzaExcepcion()
        {
            // Arrange
            var evento = CrearEventoEnBorrador();
            evento.Cancelar();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => evento.Publicar());
            Assert.Contains("estado", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Secciones

        [Fact]
        public void AgregarSeccion_ConSeccionValida_AgregaSeccion()
        {
            // Arrange
            var evento = CrearEventoEnBorrador();
            var nuevaSeccion = new Seccion("VIP", 50, new PrecioEntrada(150.00m));

            // Act
            evento.AgregarSeccion(nuevaSeccion);

            // Assert
            Assert.Equal(2, evento.Secciones.Count);
            Assert.Contains(nuevaSeccion, evento.Secciones);
        }

        [Fact]
        public void AgregarSeccion_ConSeccionDuplicada_LanzaExcepcion()
        {
            // Arrange
            var evento = CrearEventoEnBorrador();
            var seccionExistente = evento.Secciones.First();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => 
                evento.AgregarSeccion(seccionExistente));
            Assert.Contains("duplicada", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AgregarSeccion_ConSeccionNula_LanzaExcepcion()
        {
            // Arrange
            var evento = CrearEventoEnBorrador();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => evento.AgregarSeccion(null));
        }

        #endregion

        #region Finalización y Cancelación

        [Fact]
        public void Finalizar_ConEventoPublicado_CambiaEstadoAFinalizado()
        {
            // Arrange
            var evento = CrearEventoEnBorrador();
            evento.Publicar();

            // Act
            evento.Finalizar();

            // Assert
            Assert.True(evento.Estado.EsFinalizado);
        }

        [Fact]
        public void Finalizar_ConEventoEnBorrador_LanzaExcepcion()
        {
            // Arrange
            var evento = CrearEventoEnBorrador();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => evento.Finalizar());
            Assert.Contains("estado", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Cancelar_ConEventoEnBorrador_CambiaEstadoACancelado()
        {
            // Arrange
            var evento = CrearEventoEnBorrador();

            // Act
            evento.Cancelar();

            // Assert
            Assert.True(evento.Estado.EsCancelado);
        }

        [Fact]
        public void Cancelar_ConEventoPublicado_CambiaEstadoACancelado()
        {
            // Arrange
            var evento = CrearEventoEnBorrador();
            evento.Publicar();

            // Act
            evento.Cancelar();

            // Assert
            Assert.True(evento.Estado.EsCancelado);
        }

        #endregion

        #region Helpers

        private Evento CrearEventoEnBorrador()
        {
            var nombre = "Concierto de Rock";
            var descripcion = "Un concierto increíble";
            var fecha = DateTime.UtcNow.AddDays(30);
            var duracion = new DuracionEvento(2, 30);
            var secciones = new List<Seccion>
            {
                new Seccion("General", 500, new PrecioEntrada(50.00m))
            };

            return Evento.Crear(nombre, descripcion, fecha, duracion, secciones);
        }

        #endregion
    }
}

