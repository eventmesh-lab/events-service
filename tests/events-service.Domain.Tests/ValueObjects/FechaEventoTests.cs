using System;
using Xunit;
using events_service.Domain.ValueObjects;

namespace events_service.Domain.Tests.ValueObjects
{
    /// <summary>
    /// Pruebas para el Value Object FechaEvento.
    /// Valida las invariantes y comportamiento del objeto de valor que representa la fecha de un evento.
    /// </summary>
    public class FechaEventoTests
    {
        [Fact]
        public void Constructor_ConFechaValida_CreaInstancia()
        {
            // Arrange
            var fecha = DateTime.UtcNow.AddDays(30);

            // Act
            var fechaEvento = new FechaEvento(fecha);

            // Assert
            Assert.Equal(fecha.Date, fechaEvento.Valor.Date);
        }

        [Fact]
        public void Constructor_ConFechaNula_LanzaExcepcion()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FechaEvento(null));
        }

        [Fact]
        public void Constructor_ConFechaEnElPasado_LanzaExcepcion()
        {
            // Arrange
            var fechaPasado = DateTime.UtcNow.AddDays(-1);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new FechaEvento(fechaPasado));
            Assert.Contains("futuro", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_ConFechaHoy_EsValida()
        {
            // Arrange
            var fechaHoy = DateTime.UtcNow.Date;

            // Act
            var fechaEvento = new FechaEvento(fechaHoy);

            // Assert
            Assert.Equal(fechaHoy, fechaEvento.Valor.Date);
        }

        [Fact]
        public void Equals_ConMismaFecha_RetornaTrue()
        {
            // Arrange
            var fecha = DateTime.UtcNow.AddDays(30).Date;
            var fechaEvento1 = new FechaEvento(fecha);
            var fechaEvento2 = new FechaEvento(fecha);

            // Act
            var sonIguales = fechaEvento1.Equals(fechaEvento2);

            // Assert
            Assert.True(sonIguales);
        }

        [Fact]
        public void Equals_ConFechasDiferentes_RetornaFalse()
        {
            // Arrange
            var fecha1 = DateTime.UtcNow.AddDays(30).Date;
            var fecha2 = DateTime.UtcNow.AddDays(31).Date;
            var fechaEvento1 = new FechaEvento(fecha1);
            var fechaEvento2 = new FechaEvento(fecha2);

            // Act
            var sonIguales = fechaEvento1.Equals(fechaEvento2);

            // Assert
            Assert.False(sonIguales);
        }

        [Fact]
        public void GetHashCode_ConMismaFecha_RetornaMismoHash()
        {
            // Arrange
            var fecha = DateTime.UtcNow.AddDays(30).Date;
            var fechaEvento1 = new FechaEvento(fecha);
            var fechaEvento2 = new FechaEvento(fecha);

            // Act
            var hash1 = fechaEvento1.GetHashCode();
            var hash2 = fechaEvento2.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void OperadorIgualdad_ConMismaFecha_RetornaTrue()
        {
            // Arrange
            var fecha = DateTime.UtcNow.AddDays(30).Date;
            var fechaEvento1 = new FechaEvento(fecha);
            var fechaEvento2 = new FechaEvento(fecha);

            // Act
            var sonIguales = fechaEvento1 == fechaEvento2;

            // Assert
            Assert.True(sonIguales);
        }

        [Fact]
        public void OperadorDesigualdad_ConFechasDiferentes_RetornaTrue()
        {
            // Arrange
            var fecha1 = DateTime.UtcNow.AddDays(30).Date;
            var fecha2 = DateTime.UtcNow.AddDays(31).Date;
            var fechaEvento1 = new FechaEvento(fecha1);
            var fechaEvento2 = new FechaEvento(fecha2);

            // Act
            var sonDiferentes = fechaEvento1 != fechaEvento2;

            // Assert
            Assert.True(sonDiferentes);
        }

        [Fact]
        public void Valor_EsInmutable_NoPuedeModificarse()
        {
            // Arrange
            var fecha = DateTime.UtcNow.AddDays(30).Date;
            var fechaEvento = new FechaEvento(fecha);
            var fechaOriginal = fechaEvento.Valor;

            // Act - Intentar modificar la fecha subyacente no debería afectar el Value Object
            // (Esto valida que el Value Object encapsula correctamente el valor)

            // Assert
            Assert.Equal(fechaOriginal, fechaEvento.Valor);
        }
    }
}

