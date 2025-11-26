using System;
using Xunit;
using events_service.Domain.ValueObjects;

namespace events_service.Domain.Tests.ValueObjects
{
    /// <summary>
    /// Pruebas para el Value Object EstadoEvento.
    /// Valida las invariantes y comportamiento del objeto de valor que representa el estado de un evento.
    /// </summary>
    public class EstadoEventoTests
    {
        [Fact]
        public void Constructor_ConEstadoValido_CreaInstancia()
        {
            // Arrange
            var estado = "Borrador";

            // Act
            var estadoEvento = new EstadoEvento(estado);

            // Assert
            Assert.Equal(estado, estadoEvento.Valor);
        }

        [Fact]
        public void Constructor_ConEstadoInvalido_LanzaExcepcion()
        {
            // Arrange
            var estadoInvalido = "EstadoInvalido";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new EstadoEvento(estadoInvalido));
            Assert.Contains("estado", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_ConEstadoNulo_LanzaExcepcion()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new EstadoEvento(null));
        }

        [Fact]
        public void Constructor_ConEstadoVacio_LanzaExcepcion()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new EstadoEvento(string.Empty));
            Assert.Contains("estado", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("Borrador")]
        [InlineData("PendientePago")]
        [InlineData("Publicado")]
        [InlineData("EnCurso")]
        [InlineData("Finalizado")]
        [InlineData("Cancelado")]
        public void Constructor_ConEstadosValidos_CreaInstancia(string estado)
        {
            // Act
            var estadoEvento = new EstadoEvento(estado);

            // Assert
            Assert.Equal(estado, estadoEvento.Valor);
        }

        [Fact]
        public void EsBorrador_ConEstadoBorrador_RetornaTrue()
        {
            // Arrange
            var estado = new EstadoEvento("Borrador");

            // Act
            var esBorrador = estado.EsBorrador;

            // Assert
            Assert.True(esBorrador);
        }

        [Fact]
        public void EsPendientePago_ConEstadoPendientePago_RetornaTrue()
        {
            // Arrange
            var estado = new EstadoEvento("PendientePago");

            // Act & Assert
            Assert.True(estado.EsPendientePago);
        }

        [Fact]
        public void EsPublicado_ConEstadoPublicado_RetornaTrue()
        {
            // Arrange
            var estado = new EstadoEvento("Publicado");

            // Act
            var esPublicado = estado.EsPublicado;

            // Assert
            Assert.True(esPublicado);
        }

        [Fact]
        public void EsEnCurso_ConEstadoEnCurso_RetornaTrue()
        {
            // Arrange
            var estado = new EstadoEvento("EnCurso");

            // Act & Assert
            Assert.True(estado.EsEnCurso);
        }

        [Fact]
        public void EsFinalizado_ConEstadoFinalizado_RetornaTrue()
        {
            // Arrange
            var estado = new EstadoEvento("Finalizado");

            // Act
            var esFinalizado = estado.EsFinalizado;

            // Assert
            Assert.True(esFinalizado);
        }

        [Fact]
        public void EsCancelado_ConEstadoCancelado_RetornaTrue()
        {
            // Arrange
            var estado = new EstadoEvento("Cancelado");

            // Act
            var esCancelado = estado.EsCancelado;

            // Assert
            Assert.True(esCancelado);
        }

        [Fact]
        public void PuedeTransicionarA_DeBorradorAPendientePago_RetornaTrue()
        {
            // Arrange
            var estadoBorrador = new EstadoEvento("Borrador");
            var estadoPendientePago = new EstadoEvento("PendientePago");

            // Act
            var puedeTransicionar = estadoBorrador.PuedeTransicionarA(estadoPendientePago);

            // Assert
            Assert.True(puedeTransicionar);
        }

        [Fact]
        public void PuedeTransicionarA_DePendientePagoAPublicado_RetornaTrue()
        {
            // Arrange
            var estadoPendientePago = new EstadoEvento("PendientePago");
            var estadoPublicado = new EstadoEvento("Publicado");

            // Act
            var puedeTransicionar = estadoPendientePago.PuedeTransicionarA(estadoPublicado);

            // Assert
            Assert.True(puedeTransicionar);
        }

        [Fact]
        public void PuedeTransicionarA_DePublicadoAEnCurso_RetornaTrue()
        {
            // Arrange
            var estadoPublicado = new EstadoEvento("Publicado");
            var estadoEnCurso = new EstadoEvento("EnCurso");

            // Act
            var puedeTransicionar = estadoPublicado.PuedeTransicionarA(estadoEnCurso);

            // Assert
            Assert.True(puedeTransicionar);
        }

        [Fact]
        public void PuedeTransicionarA_DeEnCursoAFinalizado_RetornaTrue()
        {
            // Arrange
            var estadoEnCurso = new EstadoEvento("EnCurso");
            var estadoFinalizado = new EstadoEvento("Finalizado");

            // Act
            var puedeTransicionar = estadoEnCurso.PuedeTransicionarA(estadoFinalizado);

            // Assert
            Assert.True(puedeTransicionar);
        }

        [Fact]
        public void PuedeTransicionarA_DeBorradorACancelado_RetornaTrue()
        {
            // Arrange
            var estadoBorrador = new EstadoEvento("Borrador");
            var estadoCancelado = new EstadoEvento("Cancelado");

            // Act
            var puedeTransicionar = estadoBorrador.PuedeTransicionarA(estadoCancelado);

            // Assert
            Assert.True(puedeTransicionar);
        }

        [Fact]
        public void PuedeTransicionarA_DePendientePagoABorrador_RetornaFalse()
        {
            // Arrange
            var estadoPendientePago = new EstadoEvento("PendientePago");
            var estadoBorrador = new EstadoEvento("Borrador");

            // Act
            var puedeTransicionar = estadoPendientePago.PuedeTransicionarA(estadoBorrador);

            // Assert
            Assert.False(puedeTransicionar);
        }

        [Fact]
        public void PuedeTransicionarA_DePublicadoABorrador_RetornaFalse()
        {
            // Arrange
            var estadoPublicado = new EstadoEvento("Publicado");
            var estadoBorrador = new EstadoEvento("Borrador");

            // Act
            var puedeTransicionar = estadoPublicado.PuedeTransicionarA(estadoBorrador);

            // Assert
            Assert.False(puedeTransicionar);
        }

        [Fact]
        public void PuedeTransicionarA_DeFinalizadoAPublicado_RetornaFalse()
        {
            // Arrange
            var estadoFinalizado = new EstadoEvento("Finalizado");
            var estadoPublicado = new EstadoEvento("Publicado");

            // Act
            var puedeTransicionar = estadoFinalizado.PuedeTransicionarA(estadoPublicado);

            // Assert
            Assert.False(puedeTransicionar);
        }

        [Fact]
        public void Equals_ConMismoEstado_RetornaTrue()
        {
            // Arrange
            var estado1 = new EstadoEvento("Borrador");
            var estado2 = new EstadoEvento("Borrador");

            // Act
            var sonIguales = estado1.Equals(estado2);

            // Assert
            Assert.True(sonIguales);
        }

        [Fact]
        public void Equals_ConEstadosDiferentes_RetornaFalse()
        {
            // Arrange
            var estado1 = new EstadoEvento("Borrador");
            var estado2 = new EstadoEvento("Publicado");

            // Act
            var sonIguales = estado1.Equals(estado2);

            // Assert
            Assert.False(sonIguales);
        }

        [Fact]
        public void GetHashCode_ConMismoEstado_RetornaMismoHash()
        {
            // Arrange
            var estado1 = new EstadoEvento("Borrador");
            var estado2 = new EstadoEvento("Borrador");

            // Act
            var hash1 = estado1.GetHashCode();
            var hash2 = estado2.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void OperadorIgualdad_ConMismoEstado_RetornaTrue()
        {
            // Arrange
            var estado1 = new EstadoEvento("Borrador");
            var estado2 = new EstadoEvento("Borrador");

            // Act
            var sonIguales = estado1 == estado2;

            // Assert
            Assert.True(sonIguales);
        }

        [Fact]
        public void OperadorDesigualdad_ConEstadosDiferentes_RetornaTrue()
        {
            // Arrange
            var estado1 = new EstadoEvento("Borrador");
            var estado2 = new EstadoEvento("Publicado");

            // Act
            var sonDiferentes = estado1 != estado2;

            // Assert
            Assert.True(sonDiferentes);
        }
    }
}

