using System;
using Xunit;
using events_service.Domain.ValueObjects;

namespace events_service.Domain.Tests.ValueObjects
{
    /// <summary>
    /// Pruebas para el Value Object DuracionEvento.
    /// Valida las invariantes y comportamiento del objeto de valor que representa la duración de un evento.
    /// </summary>
    public class DuracionEventoTests
    {
        [Fact]
        public void Constructor_ConDuracionPositiva_CreaInstancia()
        {
            // Arrange
            var horas = 2;
            var minutos = 30;

            // Act
            var duracion = new DuracionEvento(horas, minutos);

            // Assert
            Assert.Equal(horas, duracion.Horas);
            Assert.Equal(minutos, duracion.Minutos);
        }

        [Fact]
        public void Constructor_ConHorasNegativas_LanzaExcepcion()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new DuracionEvento(-1, 0));
            Assert.Contains("positiva", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_ConMinutosNegativos_LanzaExcepcion()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new DuracionEvento(1, -1));
            Assert.Contains("positivo", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_ConMinutosMayorA59_LanzaExcepcion()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new DuracionEvento(1, 60));
            Assert.Contains("minutos", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_ConHorasCeroYMinutosCero_LanzaExcepcion()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new DuracionEvento(0, 0));
            Assert.Contains("duración", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_ConDuracionDeVeinticuatroHoras_EsValido()
        {
            // Arrange & Act
            var duracion = new DuracionEvento(24, 0);

            // Assert
            Assert.Equal(24, duracion.Horas);
            Assert.Equal(0, duracion.Minutos);
        }

        [Fact]
        public void Constructor_ConHorasCeroYMinutosPositivos_EsValido()
        {
            // Arrange
            var minutos = 30;

            // Act
            var duracion = new DuracionEvento(0, minutos);

            // Assert
            Assert.Equal(0, duracion.Horas);
            Assert.Equal(minutos, duracion.Minutos);
        }

        [Fact]
        public void TotalMinutos_CalculaCorrectamente()
        {
            // Arrange
            var horas = 2;
            var minutos = 30;
            var duracion = new DuracionEvento(horas, minutos);

            // Act
            var totalMinutos = duracion.TotalMinutos;

            // Assert
            Assert.Equal((horas * 60) + minutos, totalMinutos);
        }

        [Fact]
        public void ComoTimeSpan_RegresaTimeSpanEquivalente()
        {
            // Arrange
            var duracion = new DuracionEvento(1, 45);

            // Act
            var timespan = duracion.ComoTimeSpan;

            // Assert
            Assert.Equal(TimeSpan.FromMinutes(105), timespan);
        }

        [Fact]
        public void Equals_ConMismaDuracion_RetornaTrue()
        {
            // Arrange
            var duracion1 = new DuracionEvento(2, 30);
            var duracion2 = new DuracionEvento(2, 30);

            // Act
            var sonIguales = duracion1.Equals(duracion2);

            // Assert
            Assert.True(sonIguales);
        }

        [Fact]
        public void Equals_ConDuracionesDiferentes_RetornaFalse()
        {
            // Arrange
            var duracion1 = new DuracionEvento(2, 30);
            var duracion2 = new DuracionEvento(3, 0);

            // Act
            var sonIguales = duracion1.Equals(duracion2);

            // Assert
            Assert.False(sonIguales);
        }

        [Fact]
        public void GetHashCode_ConMismaDuracion_RetornaMismoHash()
        {
            // Arrange
            var duracion1 = new DuracionEvento(2, 30);
            var duracion2 = new DuracionEvento(2, 30);

            // Act
            var hash1 = duracion1.GetHashCode();
            var hash2 = duracion2.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void OperadorIgualdad_ConMismaDuracion_RetornaTrue()
        {
            // Arrange
            var duracion1 = new DuracionEvento(2, 30);
            var duracion2 = new DuracionEvento(2, 30);

            // Act
            var sonIguales = duracion1 == duracion2;

            // Assert
            Assert.True(sonIguales);
        }

        [Fact]
        public void OperadorDesigualdad_ConDuracionesDiferentes_RetornaTrue()
        {
            // Arrange
            var duracion1 = new DuracionEvento(2, 30);
            var duracion2 = new DuracionEvento(3, 0);

            // Act
            var sonDiferentes = duracion1 != duracion2;

            // Assert
            Assert.True(sonDiferentes);
        }
    }
}

