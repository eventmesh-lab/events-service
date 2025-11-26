# Plan de Pruebas TDD: Events Service

Este documento describe las pruebas planificadas para el desarrollo del microservicio `events-service` siguiendo la metodología TDD (Test-Driven Development).

## Estrategia TDD

Seguimos el ciclo **Red → Green → Refactor**:

1. **Red**: Escribir pruebas que fallen (definir comportamiento esperado)
2. **Green**: Implementar código mínimo para que las pruebas pasen
3. **Refactor**: Mejorar el código manteniendo las pruebas verdes

## Pruebas del Dominio (Domain Layer)

### Value Objects

#### FechaEvento

**Ubicación**: `tests/events-service.Domain.Tests/ValueObjects/FechaEventoTests.cs`

**Pruebas planificadas**:

- ✅ Constructor con fecha válida crea instancia
- ✅ Constructor con fecha nula lanza excepción
- ✅ Constructor con fecha en el pasado lanza excepción
- ✅ Constructor con fecha hoy es válida
- ✅ Equals con misma fecha retorna true
- ✅ Equals con fechas diferentes retorna false
- ✅ GetHashCode con misma fecha retorna mismo hash
- ✅ Operador igualdad con misma fecha retorna true
- ✅ Operador desigualdad con fechas diferentes retorna true
- ✅ Valor es inmutable

#### DuracionEvento

**Ubicación**: `tests/events-service.Domain.Tests/ValueObjects/DuracionEventoTests.cs`

**Pruebas planificadas**:

- ✅ Constructor con duración positiva crea instancia
- ✅ Constructor con horas negativas lanza excepción
- ✅ Constructor con minutos negativos lanza excepción
- ✅ Constructor con minutos mayor a 59 lanza excepción
- ✅ Constructor con horas cero y minutos cero lanza excepción
- ✅ Constructor con horas cero y minutos positivos es válido
- ✅ TotalMinutos calcula correctamente
- ✅ Equals con misma duración retorna true
- ✅ Equals con duraciones diferentes retorna false
- ✅ GetHashCode con misma duración retorna mismo hash
- ✅ Operador igualdad con misma duración retorna true
- ✅ Operador desigualdad con duraciones diferentes retorna true

#### EstadoEvento

**Ubicación**: `tests/events-service.Domain.Tests/ValueObjects/EstadoEventoTests.cs`

**Pruebas planificadas**:

- ✅ Constructor con estado válido crea instancia
- ✅ Constructor con estado inválido lanza excepción
- ✅ Constructor con estado nulo lanza excepción
- ✅ Constructor con estado vacío lanza excepción
- ✅ Constructor con estados válidos (Borrador, Publicado, Finalizado, Cancelado)
- ✅ EsBorrador con estado Borrador retorna true
- ✅ EsPublicado con estado Publicado retorna true
- ✅ EsFinalizado con estado Finalizado retorna true
- ✅ EsCancelado con estado Cancelado retorna true
- ✅ PuedeTransicionarA de Borrador a Publicado retorna true
- ✅ PuedeTransicionarA de Publicado a Finalizado retorna true
- ✅ PuedeTransicionarA de Borrador a Cancelado retorna true
- ✅ PuedeTransicionarA de Publicado a Borrador retorna false
- ✅ PuedeTransicionarA de Finalizado a Publicado retorna false
- ✅ Equals con mismo estado retorna true
- ✅ Equals con estados diferentes retorna false
- ✅ GetHashCode con mismo estado retorna mismo hash
- ✅ Operador igualdad con mismo estado retorna true
- ✅ Operador desigualdad con estados diferentes retorna true

#### PrecioEntrada

**Ubicación**: `tests/events-service.Domain.Tests/ValueObjects/PrecioEntradaTests.cs`

**Pruebas planificadas**:

- ✅ Constructor con precio positivo crea instancia
- ✅ Constructor con precio cero es válido
- ✅ Constructor con precio negativo lanza excepción
- ✅ Constructor con precio con más de dos decimales redondea correctamente
- ✅ Constructor con precio muy alto es válido
- ✅ EsGratis con precio cero retorna true
- ✅ EsGratis con precio positivo retorna false
- ✅ Equals con mismo precio retorna true
- ✅ Equals con precios diferentes retorna false
- ✅ GetHashCode con mismo precio retorna mismo hash
- ✅ Operador igualdad con mismo precio retorna true
- ✅ Operador desigualdad con precios diferentes retorna true
- ✅ ToString formatea correctamente

### Entidades

#### Seccion

**Ubicación**: `tests/events-service.Domain.Tests/Entities/SeccionTests.cs`

**Pruebas planificadas**:

- ✅ Constructor con parámetros válidos crea instancia
- ✅ Constructor con nombre nulo lanza excepción
- ✅ Constructor con nombre vacío lanza excepción
- ✅ Constructor con capacidad cero lanza excepción
- ✅ Constructor con capacidad negativa lanza excepción
- ✅ Constructor con precio nulo lanza excepción
- ✅ Constructor con precio gratis es válido
- ✅ Equals con mismo ID retorna true
- ✅ Equals con IDs diferentes retorna false

### Agregados

#### Evento

**Ubicación**: `tests/events-service.Domain.Tests/Entities/EventoTests.cs`

**Pruebas planificadas**:

**Creación**:

- ✅ Crear con parámetros válidos crea evento en estado Borrador
- ✅ Crear con nombre nulo lanza excepción
- ✅ Crear con nombre vacío lanza excepción
- ✅ Crear con lista secciones vacía lanza excepción
- ✅ Crear con secciones nulas lanza excepción
- ✅ Crear genera evento de dominio EventoCreado

**Publicación**:

- ✅ Publicar con evento en Borrador cambia estado a Publicado
- ✅ Publicar con evento en Borrador genera evento de dominio EventoPublicado
- ✅ Publicar con evento ya publicado lanza excepción
- ✅ Publicar con evento finalizado lanza excepción
- ✅ Publicar con evento cancelado lanza excepción

**Secciones**:

- ✅ AgregarSeccion con sección válida agrega sección
- ✅ AgregarSeccion con sección duplicada lanza excepción
- ✅ AgregarSeccion con sección nula lanza excepción

**Finalización y Cancelación**:

- ✅ Finalizar con evento publicado cambia estado a Finalizado
- ✅ Finalizar con evento en Borrador lanza excepción
- ✅ Cancelar con evento en Borrador cambia estado a Cancelado
- ✅ Cancelar con evento publicado cambia estado a Cancelado

## Pruebas de Aplicación (Application Layer)

### Comandos y Handlers

#### CrearEventoCommand

**Ubicación**: `tests/events-service.Application.Tests/Commands/CrearEventoCommandTests.cs`

**Pruebas planificadas**:

- ✅ CrearEventoCommand con datos válidos crea comando
- ✅ CrearEventoCommandValidator con nombre vacío retorna error
- ✅ CrearEventoCommandValidator con lista secciones vacía retorna error
- ✅ CrearEventoCommandValidator con fecha en el pasado retorna error
- ✅ CrearEventoCommandHandler con comando válido crea evento
- ✅ CrearEventoCommandHandler con comando válido publica evento de dominio
- ✅ CrearEventoCommandHandler con comando inválido lanza excepción

#### PublicarEventoCommand

**Ubicación**: `tests/events-service.Application.Tests/Commands/PublicarEventoCommandTests.cs`

**Pruebas planificadas**:

- ✅ PublicarEventoCommand con ID válido crea comando
- ✅ PublicarEventoCommandValidator con ID vacío retorna error
- ✅ PublicarEventoCommandHandler con evento existente en Borrador publica evento
- ✅ PublicarEventoCommandHandler con evento existente en Borrador publica evento de dominio
- ✅ PublicarEventoCommandHandler con evento no existente lanza excepción
- ✅ PublicarEventoCommandHandler con evento ya publicado lanza excepción
- ✅ PublicarEventoCommandHandler con evento cancelado lanza excepción

## Orden de Implementación

1. **Value Objects** (sin dependencias)
   - FechaEvento
   - DuracionEvento
   - EstadoEvento
   - PrecioEntrada

2. **Entidad Seccion** (depende de PrecioEntrada)

3. **Agregado Evento** (depende de todos los Value Objects y Seccion)

4. **Eventos de Dominio** (dependen del agregado)
   - EventoCreado
   - EventoPublicado

5. **Comandos y Handlers** (dependen del dominio)
   - CrearEventoCommand
   - PublicarEventoCommand

## Notas Importantes

- Todas las pruebas están escritas siguiendo el patrón **Arrange-Act-Assert**
- Las pruebas documentan las invariantes del dominio
- Se usa **xUnit** como framework de pruebas
- Se usa **Moq** para mocks en pruebas de aplicación
- Las pruebas de validación usan **FluentValidation**
- Los handlers usan **MediatR** para publicación de eventos
