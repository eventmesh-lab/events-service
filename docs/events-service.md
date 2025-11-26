# Events Service

## 1. Descripción

Microservicio responsable de la **gestión del ciclo de vida completo de eventos**, desde su creación en estado borrador hasta su finalización. Implementa la lógica de negocio del agregado `Evento` según el lenguaje ubicuo de la plataforma.

**Bounded Context:** Gestión de Eventos

**Repository:** `eventmesh-lab/events-service`

---

## 2. Responsabilidades

- Crear y editar eventos en estado borrador
- Coordinar el proceso de pago de publicación
- Publicar eventos en el catálogo tras confirmación de pago
- Gestionar el estado del evento durante su ciclo de vida
- Administrar secciones y precios por tipo de entrada
- Emitir eventos de dominio para comunicación asíncrona

---

## 3. Modelo de Dominio

### 3.1 Agregado: Evento

**Root Aggregate:** `Evento`

#### Entidades

##### Evento
```csharp
public class Evento : AggregateRoot
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }
    public FechaEvento FechaInicio { get; private set; }
    public DuracionEvento Duracion { get; private set; }
    public EstadoEvento Estado { get; private set; }
    public Guid OrganizadorId { get; private set; }
    public Guid VenueId { get; private set; }
    public string Categoria { get; private set; }
    public IReadOnlyList<Seccion> Secciones { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaPublicacion { get; private set; }
}
```

##### Seccion
```csharp
public class Seccion : Entity
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public PrecioEntrada PrecioBase { get; private set; }
    public int Capacidad { get; private set; }
    public string TipoAsiento { get; private set; } // Numerado, General
}
```

#### Value Objects

##### FechaEvento
```csharp
public record FechaEvento
{
    public DateTime Fecha { get; init; }
    public TimeZoneInfo ZonaHoraria { get; init; }
    
    // Validaciones: Fecha no puede ser en el pasado
}
```

##### DuracionEvento
```csharp
public record DuracionEvento
{
    public TimeSpan Duracion { get; init; }
    
    // Validaciones: Duración debe ser positiva y menor a 24 horas
}
```

##### EstadoEvento
```csharp
public enum EstadoEvento
{
    Borrador,           // Evento creado, aún no pagado
    PendientePago,      // Pago iniciado, esperando confirmación
    Publicado,          // Visible en el catálogo
    EnCurso,            // Evento en ejecución
    Finalizado,         // Evento terminado
    Cancelado           // Evento cancelado por el organizador
}
```

##### PrecioEntrada
```csharp
public record PrecioEntrada
{
    public decimal Monto { get; init; }
    public string Moneda { get; init; } // USD, EUR, etc.
    
    // Validaciones: Monto debe ser positivo
}
```

---

## 4. Comandos del Dominio

### CrearEvento
**Descripción:** El organizador registra un nuevo evento en estado `Borrador`.

**Input:**
```csharp
public record CrearEventoCommand
{
    public string Nombre { get; init; }
    public string Descripcion { get; init; }
    public DateTime Fecha { get; init; }
    public int HorasDuracion { get; init; }
    public int MinutosDuracion { get; init; }
    public Guid OrganizadorId { get; init; }
    public Guid VenueId { get; init; }
    public string Categoria { get; init; }
    public decimal TarifaPublicacion { get; init; }
    public List<SeccionDto> Secciones { get; init; }
}
```

**Validaciones:**
- El organizador debe existir y estar activo
- El venue debe existir y estar disponible
- La fecha no puede ser en el pasado
- Al menos una sección debe estar definida

**Emite:** `EventoCreado`

---

### EditarEvento
**Descripción:** El organizador modifica datos del evento antes de la publicación.

**Input:**
```csharp
public record EditarEventoCommand
{
    public Guid EventoId { get; init; }
    public string Nombre { get; init; }
    public string Descripcion { get; init; }
    public DateTime Fecha { get; init; }
    public int HorasDuracion { get; init; }
    public int MinutosDuracion { get; init; }
    public string Categoria { get; init; }
    public List<SeccionDto> Secciones { get; init; }

    public record SeccionDto
    {
        public Guid? Id { get; init; }
        public string Nombre { get; init; }
        public int Capacidad { get; init; }
        public decimal Precio { get; init; }
    }
}
```

**Validaciones:**
- El evento debe estar en estado `Borrador`
- El organizador debe ser el propietario del evento

**Emite:** `EventoEditado`

---

### PagarPublicacion
**Descripción:** El organizador inicia el pago que habilita la publicación del evento.

**Input:**
```csharp
public record PagarPublicacionCommand
{
    public Guid EventoId { get; init; }
    public Guid TransaccionPagoId { get; init; }
    public decimal Monto { get; init; }
}
```

**Validaciones:**
- El evento debe estar en estado `Borrador`
- El monto del pago debe coincidir con la tarifa de publicación

**Emite:** `PagoPublicacionIniciado`

**Estado resultante:** `PendientePago`

---

### PublicarEvento
**Descripción:** El sistema marca el evento como publicado tras confirmar el pago.

**Input:**
```csharp
public record PublicarEventoCommand
{
    public Guid EventoId { get; init; }
    public Guid PagoConfirmadoId { get; init; }
}
```

**Validaciones:**
- El evento debe estar en estado `PendientePago`
- El pago debe estar confirmado

**Emite:** `EventoPublicado`

**Estado resultante:** `Publicado`

---

### IniciarEvento
**Descripción:** El sistema marca el evento como en curso cuando llega la fecha de inicio.

**Input:**
```csharp
public record IniciarEventoCommand
{
    public Guid EventoId { get; init; }
}
```

**Validaciones:**
- El evento debe estar en estado `Publicado`
- La fecha actual debe ser posterior o igual a la fecha de inicio

**Emite:** `EventoIniciado`

**Estado resultante:** `EnCurso`

---

### FinalizarEvento
**Descripción:** El sistema marca el evento como finalizado.

**Input:**
```csharp
public record FinalizarEventoCommand
{
    public Guid EventoId { get; init; }
}
```

**Validaciones:**
- El evento debe estar en estado `EnCurso`

**Emite:** `EventoFinalizado`

**Estado resultante:** `Finalizado`

---

## 5. Eventos de Dominio

### EventoCreado
```csharp
public record EventoCreado : DomainEvent
{
    public Guid EventoId { get; init; }
    public string Nombre { get; init; }
    public Guid OrganizadorId { get; init; }
    public DateTime FechaInicio { get; init; }
    public DateTime FechaCreacion { get; init; }
}
```

**Suscriptores:**
- `notifications-service`: Envía confirmación al organizador

---

### PagoPublicacionIniciado
```csharp
public record PagoPublicacionIniciado : DomainEvent
{
    public Guid EventoId { get; init; }
    public Guid TransaccionPagoId { get; init; }
    public decimal Monto { get; init; }
}
```

**Suscriptores:**
- `payments-service`: Procesa el pago

---

### EventoPublicado
```csharp
public record EventoPublicado : DomainEvent
{
    public Guid EventoId { get; init; }
    public string Nombre { get; init; }
    public Guid OrganizadorId { get; init; }
    public DateTime FechaPublicacion { get; init; }
}
```

**Suscriptores:**
- `notifications-service`: Notifica al organizador
- `analytics-service`: Registra métrica
- `forums-service`: Crea foro del evento

---

### EventoEditado
```csharp
public record EventoEditado : DomainEvent
{
    public Guid EventoId { get; init; }
    public DateTime FechaEdicion { get; init; }
}
```

---

### EventoIniciado
```csharp
public record EventoIniciado : DomainEvent
{
    public Guid EventoId { get; init; }
    public DateTime FechaInicio { get; init; }
}
```

**Suscriptores:**
- `streaming-service`: Habilita transmisión (si aplica)
- `notifications-service`: Recordatorio a asistentes

---

### EventoFinalizado
```csharp
public record EventoFinalizado : DomainEvent
{
    public Guid EventoId { get; init; }
    public DateTime FechaFinalizacion { get; init; }
}
```

**Suscriptores:**
- `analytics-service`: Genera reporte de asistencia
- `streaming-service`: Cierra transmisión

---

## 6. Reglas de Negocio

1. **Publicación requiere pago confirmado:** Un evento solo puede pasar de `Borrador` a `Publicado` si existe un pago confirmado de publicación.

2. **Edición limitada:** Solo se pueden editar eventos en estado `Borrador`. Una vez publicados, ciertos campos quedan bloqueados.

3. **Capacidad por sección:** La suma de capacidades de todas las secciones define el aforo máximo del evento.

4. **Fecha de inicio futura:** Al crear o editar, la fecha de inicio debe ser posterior a la fecha actual.

5. **Organizador autorizado:** Solo el organizador propietario puede editar o cancelar el evento.

---

## 7. Integraciones

### Servicios de Dominio

#### ServicioDePublicacion
```csharp
public interface IServicioDePublicacion
{
    Task<Result> ValidarYPublicarEvento(Guid eventoId, Guid pagoId);
}
```

Coordina la validación del pago y la transición de estado del evento.

---

### Comunicación Asíncrona (RabbitMQ)

**Exchange:** `eventos.domain.events`

**Publica:**
- `EventoCreado`
- `EventoPublicado`
- `EventoEditado`
- `EventoIniciado`
- `EventoFinalizado`

**Consume:**
- `PagoPublicacionConfirmado` (desde `payments-service`)

---

## 8. Persistencia

**Base de Datos:** PostgreSQL

### Tabla: eventos
```sql
CREATE TABLE eventos (
    id UUID PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    descripcion TEXT,
    fecha_inicio TIMESTAMP NOT NULL,
    duracion INTERVAL NOT NULL,
    estado VARCHAR(20) NOT NULL,
    organizador_id UUID NOT NULL,
    venue_id UUID NOT NULL,
    categoria VARCHAR(100),
    fecha_creacion TIMESTAMP NOT NULL,
    fecha_publicacion TIMESTAMP,
    version INT NOT NULL
);
```

### Tabla: secciones
```sql
CREATE TABLE secciones (
    id UUID PRIMARY KEY,
    evento_id UUID NOT NULL REFERENCES eventos(id),
    nombre VARCHAR(100) NOT NULL,
    precio_monto DECIMAL(10,2) NOT NULL,
    precio_moneda VARCHAR(3) NOT NULL,
    capacidad INT NOT NULL,
    tipo_asiento VARCHAR(20) NOT NULL
);
```

---

## 9. API Endpoints

### POST /api/eventos
Crea un nuevo evento en estado borrador.

**Request:**
```json
{
  "nombre": "Concierto Rock 2025",
  "descripcion": "Gran concierto de rock",
  "fechaInicio": "2025-12-15T20:00:00Z",
  "duracion": "03:00:00",
  "organizadorId": "uuid",
  "venueId": "uuid",
  "categoria": "Música",
  "secciones": [
    {
      "nombre": "VIP",
      "precioMonto": 150.00,
      "precioMoneda": "USD",
      "capacidad": 100,
      "tipoAsiento": "Numerado"
    }
  ]
}
```

**Response:** `201 Created`

---

### PUT /api/eventos/{id}
Edita un evento existente en estado borrador.

---

### POST /api/eventos/{id}/pagar-publicacion
Inicia el proceso de pago de publicación.

---

### GET /api/eventos/{id}
Obtiene los detalles de un evento.

---

### GET /api/eventos
Lista eventos publicados con filtros y paginación.

**Query Params:**
- `categoria`: Filtra por categoría
- `fechaDesde`: Filtra por fecha mínima
- `organizadorId`: Filtra por organizador
- `page`: Número de página
- `pageSize`: Tamaño de página

---

## 10. Tecnologías

- **.NET 8** (Minimal APIs)
- **Entity Framework Core 8** (PostgreSQL)
- **MediatR** (CQRS pattern)
- **FluentValidation** (Validaciones)
- **RabbitMQ.Client** (Mensajería)
- **Serilog** (Logging)
- **OpenTelemetry** (Observabilidad)

---

## 11. Observabilidad

### Métricas
- `eventos_creados_total`: Contador de eventos creados
- `eventos_publicados_total`: Contador de eventos publicados
- `tiempo_publicacion_segundos`: Histograma de tiempo entre creación y publicación

### Logs Estructurados
- `EventoCreado`: Nivel INFO
- `EventoPublicado`: Nivel INFO
- `ErrorValidacion`: Nivel WARNING
- `ErrorIntegracion`: Nivel ERROR

### Traces
- Span principal: `POST /api/eventos`
- Spans secundarios: Validaciones, persistencia, publicación de eventos

---

## 12. Testing

### Pruebas Unitarias
- Comandos y validaciones
- Lógica de agregados
- Value objects

### Pruebas de Integración
- Endpoints API
- Persistencia en PostgreSQL
- Publicación de eventos en RabbitMQ

### Pruebas de Contratos
- Schema de eventos de dominio

---

## 13. Referencias

- [Lenguaje Ubicuo](../lenguaje-ubicuo.md)
- [ADR-0004: Microservice Template](../adr/0004-microservice-template.md)
- [Venues Service](venues-service.md)
- [Payments Service](payments-service.md)
