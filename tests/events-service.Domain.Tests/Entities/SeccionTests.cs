using System;
using Xunit;
using events_service.Domain.Entities;
using events_service.Domain.ValueObjects;

namespace events_service.Domain.Tests.Entities
{
    /// <summary>
    /// Pruebas para la entidad Seccion.
    /// Valida las invariantes y comportamiento de una sección de un evento.
    /// </summary>
    public class SeccionTests
    {
        [Fact]
        public void Constructor_ConParametrosValidos_CreaInstancia()
        {
            // Arrange
            var nombre = "VIP";
            var capacidad = 100;
            var precio = new PrecioEntrada(150.00m);

            // Act
            var seccion = new Seccion(nombre, capacidad, precio);

            // Assert
            Assert.Equal(nombre, seccion.Nombre);
            Assert.Equal(capacidad, seccion.Capacidad);
            Assert.Equal(precio, seccion.Precio);
            Assert.NotEqual(Guid.Empty, seccion.Id);
        }

        [Fact]
        public void Constructor_ConNombreNulo_LanzaExcepcion()
        {
            // Arrange
            var capacidad = 100;
            var precio = new PrecioEntrada(150.00m);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Seccion(null, capacidad, precio));
        }

        [Fact]
        public void Constructor_ConNombreVacio_LanzaExcepcion()
        {
            // Arrange
            var capacidad = 100;
            var precio = new PrecioEntrada(150.00m);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new Seccion(string.Empty, capacidad, precio));
            Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_ConCapacidadCero_LanzaExcepcion()
        {
            // Arrange
            var nombre = "VIP";
            var precio = new PrecioEntrada(150.00m);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new Seccion(nombre, 0, precio));
            Assert.Contains("capacidad", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_ConCapacidadNegativa_LanzaExcepcion()
        {
            // Arrange
            var nombre = "VIP";
            var precio = new PrecioEntrada(150.00m);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new Seccion(nombre, -1, precio));
            Assert.Contains("capacidad", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_ConPrecioNulo_LanzaExcepcion()
        {
            // Arrange
            var nombre = "VIP";
            var capacidad = 100;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Seccion(nombre, capacidad, null!));
        }

        [Fact]
        public void Constructor_ConPrecioGratis_EsValido()
        {
            // Arrange
            var nombre = "General";
            var capacidad = 500;
            var precio = new PrecioEntrada(0m);

            // Act
            var seccion = new Seccion(nombre, capacidad, precio);

            // Assert
            Assert.True(seccion.Precio.EsGratis);
        }

        [Fact]
        public void Equals_ConMismoId_RetornaTrue()
        {
            // Arrange
            var id = Guid.NewGuid();
            var nombre = "VIP";
            var capacidad = 100;
            var precio = new PrecioEntrada(150.00m);

            var seccion1 = new Seccion(id, nombre, capacidad, precio);
            var seccion2 = new Seccion(id, nombre, capacidad, precio);

            // Act
            var sonIguales = seccion1.Equals(seccion2);

            // Assert
            Assert.True(sonIguales);
        }

        [Fact]
        public void Equals_ConIdsDiferentes_RetornaFalse()
        {
            // Arrange
            var nombre = "VIP";
            var capacidad = 100;
            var precio = new PrecioEntrada(150.00m);

            var seccion1 = new Seccion(nombre, capacidad, precio);
            var seccion2 = new Seccion(nombre, capacidad, precio);

            // Act
            var sonIguales = seccion1.Equals(seccion2);

            // Assert
            Assert.False(sonIguales);
        }
    }
}

