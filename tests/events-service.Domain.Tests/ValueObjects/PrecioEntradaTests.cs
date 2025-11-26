using System;
using Xunit;
using events_service.Domain.ValueObjects;

namespace events_service.Domain.Tests.ValueObjects
{
    /// <summary>
    /// Pruebas para el Value Object PrecioEntrada.
    /// Valida las invariantes y comportamiento del objeto de valor que representa el precio de entrada a un evento.
    /// </summary>
    public class PrecioEntradaTests
    {
        [Fact]
        public void Constructor_ConPrecioPositivo_CreaInstancia()
        {
            // Arrange
            var precio = 100.50m;

            // Act
            var precioEntrada = new PrecioEntrada(precio);

            // Assert
            Assert.Equal(precio, precioEntrada.Valor);
        }

        [Fact]
        public void Constructor_ConPrecioCero_EsValido()
        {
            // Arrange
            var precio = 0m;

            // Act
            var precioEntrada = new PrecioEntrada(precio);

            // Assert
            Assert.Equal(precio, precioEntrada.Valor);
        }

        [Fact]
        public void Constructor_ConPrecioNegativo_LanzaExcepcion()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new PrecioEntrada(-10m));
            Assert.Contains("negativo", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_ConPrecioConMasDeDosDecimales_RedondeaCorrectamente()
        {
            // Arrange
            var precio = 100.999m;

            // Act
            var precioEntrada = new PrecioEntrada(precio);

            // Assert
            Assert.Equal(101.00m, precioEntrada.Valor);
        }

        [Fact]
        public void Constructor_ConPrecioMuyAlto_EsValido()
        {
            // Arrange
            var precio = 999999.99m;

            // Act
            var precioEntrada = new PrecioEntrada(precio);

            // Assert
            Assert.Equal(precio, precioEntrada.Valor);
        }

        [Fact]
        public void EsGratis_ConPrecioCero_RetornaTrue()
        {
            // Arrange
            var precio = new PrecioEntrada(0m);

            // Act
            var esGratis = precio.EsGratis;

            // Assert
            Assert.True(esGratis);
        }

        [Fact]
        public void EsGratis_ConPrecioPositivo_RetornaFalse()
        {
            // Arrange
            var precio = new PrecioEntrada(100m);

            // Act
            var esGratis = precio.EsGratis;

            // Assert
            Assert.False(esGratis);
        }

        [Fact]
        public void Equals_ConMismoPrecio_RetornaTrue()
        {
            // Arrange
            var precio1 = new PrecioEntrada(100.50m);
            var precio2 = new PrecioEntrada(100.50m);

            // Act
            var sonIguales = precio1.Equals(precio2);

            // Assert
            Assert.True(sonIguales);
        }

        [Fact]
        public void Equals_ConPreciosDiferentes_RetornaFalse()
        {
            // Arrange
            var precio1 = new PrecioEntrada(100m);
            var precio2 = new PrecioEntrada(200m);

            // Act
            var sonIguales = precio1.Equals(precio2);

            // Assert
            Assert.False(sonIguales);
        }

        [Fact]
        public void GetHashCode_ConMismoPrecio_RetornaMismoHash()
        {
            // Arrange
            var precio1 = new PrecioEntrada(100.50m);
            var precio2 = new PrecioEntrada(100.50m);

            // Act
            var hash1 = precio1.GetHashCode();
            var hash2 = precio2.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void OperadorIgualdad_ConMismoPrecio_RetornaTrue()
        {
            // Arrange
            var precio1 = new PrecioEntrada(100.50m);
            var precio2 = new PrecioEntrada(100.50m);

            // Act
            var sonIguales = precio1 == precio2;

            // Assert
            Assert.True(sonIguales);
        }

        [Fact]
        public void OperadorDesigualdad_ConPreciosDiferentes_RetornaTrue()
        {
            // Arrange
            var precio1 = new PrecioEntrada(100m);
            var precio2 = new PrecioEntrada(200m);

            // Act
            var sonDiferentes = precio1 != precio2;

            // Assert
            Assert.True(sonDiferentes);
        }

        [Fact]
        public void ToString_FormateaCorrectamente()
        {
            // Arrange
            var precio = new PrecioEntrada(100.50m);

            // Act
            var formato = precio.ToString();

            // Assert
            Assert.Contains("100.50", formato);
        }
    }
}

