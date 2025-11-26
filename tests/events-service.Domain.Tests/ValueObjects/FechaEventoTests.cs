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
            var fecha = DateTime.Now.AddDays(10);

            // Act
            var fechaEvento = new FechaEvento(fecha);

            // Assert
            Assert.Equal(fecha.Date, fechaEvento.Valor.Date);
        }

        [Fact]
        public void Constructor_ConFechaEnElPasado_LanzaExcepcion()
        {
            // Arrange
            var fechaPasado = DateTime.Now.AddDays(-1);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new FechaEvento(fechaPasado));
            Assert.Contains("futuro", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Equals_ConMismosValores_RetornaTrue()
        {
            // Arrange
            var fecha = DateTime.Now.AddDays(3);
            var fechaEvento1 = new FechaEvento(fecha);
            var fechaEvento2 = new FechaEvento(fecha);

            // Act
            var igualdad = fechaEvento1.Equals(fechaEvento2);

            // Assert
            Assert.True(igualdad);
            Assert.True(fechaEvento1 == fechaEvento2);
        }

        [Fact]
        public void Equals_ConFechasDiferentes_RetornaFalse()
        {
            // Arrange
            var fecha = DateTime.Now.AddDays(3);
            var otraFecha = DateTime.Now.AddDays(4);
            var fechaEvento1 = new FechaEvento(fecha);
            var fechaEvento2 = new FechaEvento(otraFecha);

            // Act
            var igualdad = fechaEvento1.Equals(fechaEvento2);

            // Assert
            Assert.False(igualdad);
            Assert.True(fechaEvento1 != fechaEvento2);
        }

        [Fact]
        public void HaComenzado_CuandoFechaMenorOIgualReferencia_RetornaTrue()
        {
            // Arrange
            var fecha = DateTime.Now.AddMinutes(1);
            var fechaEvento = new FechaEvento(fecha);

            // Act
            var haComenzado = fechaEvento.HaComenzado(DateTime.Now.AddMinutes(2));

            // Assert
            Assert.True(haComenzado);
        }

        [Fact]
        public void HaComenzado_CuandoFechaMayorReferencia_RetornaFalse()
        {
            // Arrange
            var fecha = DateTime.Now.AddDays(1);
            var fechaEvento = new FechaEvento(fecha);

            // Act
            var haComenzado = fechaEvento.HaComenzado(DateTime.Now);

            // Assert
            Assert.False(haComenzado);
        }
    }
}

