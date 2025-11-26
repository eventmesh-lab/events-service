# Guía de Desarrollo

Este documento describe cómo preparar, ejecutar y probar el `events-service` en entornos locales y de integración. Complementa la [ficha del servicio](https://eventmesh-lab.github.io/org-docs/services/events-service/) y la [guía técnica global](https://eventmesh-lab.github.io/org-docs/guia-tecnica/).

## 1. Prerrequisitos

- SDK **.NET 8.0** y `dotnet` CLI.
- **Docker** y **Docker Compose** para orquestar dependencias (PostgreSQL, RabbitMQ).
- Acceso a la documentación central (`org-docs`).
- Opcional: **Make** u otra herramienta para scripts repetitivos.

## 2. Estructura relevante

```text
Services/events-service/
├── src/
│   ├── events-service.Api/
│   ├── events-service.Application/
│   ├── events-service.Domain/
│   └── events-service.Infrastructure/
├── tests/
│   ├── events-service.Application.Tests/
│   ├── events-service.Domain.Tests/
│   └── events-service.Infrastructure.IntegrationTests/
├── docker-compose.yml
└── Dockerfile
```

Cada proyecto corresponde a una capa hexagonal. El dominio mantiene las invariantes del agregado `Evento` y no debe depender de tecnologías externas.

## 3. Configuración local

### 3.1 Restaurar dependencias

```bash
# Desde la raíz del servicio
cd Services/events-service

dotnet restore
```

### 3.2 Variables de entorno recomendadas

Definir en `appsettings.Development.json` o como variables del sistema:

- `ConnectionStrings__EventsDb`: cadena de conexión PostgreSQL.
- `MessageBroker__Host`: host de RabbitMQ.
- `MessageBroker__Exchange`: exchange de dominio (`eventos.domain.events`).
- `OpenTelemetry__Endpoint`: colector OTLP para traces y métricas.

### 3.3 Dependencias via Docker Compose

Extiende `docker-compose.yml` para levantar infraestructura compartida durante el desarrollo:

```yaml
services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__EventsDb=Host=postgres;Port=5432;Database=events;Username=events;Password=events
      - MessageBroker__Host=rabbitmq
      - MessageBroker__Exchange=eventos.domain.events
    depends_on:
      - postgres
      - rabbitmq
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: events
      POSTGRES_USER: events
      POSTGRES_PASSWORD: events
    ports:
      - "5432:5432"
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
```

### 3.4 Migraciones (cuando se incorpore EF Core)

```bash
cd Services/events-service
dotnet ef database update --project src/events-service.Infrastructure --startup-project src/events-service.Api
```

## 4. Ejecución

### 4.1 dotnet CLI

```bash
cd Services/events-service
dotnet run --project src/events-service.Api/events-service.Api.csproj
```

Expone la API en `https://localhost:5001` y habilita Swagger en desarrollo.

### 4.2 Docker Compose

```bash
cd Services/events-service
docker compose up --build
```

Permite validar la integración con PostgreSQL y RabbitMQ.

## 5. Pruebas

### 5.1 Unitarias (Domain y Application)

```bash
cd Services/events-service
dotnet test tests/events-service.Domain.Tests/events-service.Domain.Tests.csproj
dotnet test tests/events-service.Application.Tests/events-service.Application.Tests.csproj
```

### 5.2 Integración (Infrastructure)

```bash
cd Services/events-service
dotnet test tests/events-service.Infrastructure.IntegrationTests/events-service.Infrastructure.IntegrationTests.csproj
```

Requiere dependencias disponibles (PostgreSQL, RabbitMQ). Considera usar contenedores temporales para aislar pruebas.

### 5.3 Cobertura (opcional)

```bash
cd Services/events-service
dotnet test --collect:"Xplat Code Coverage" --results-directory ./TestResults
```

## 6. Buenas prácticas

- Mantén el dominio libre de dependencias de infraestructura y documenta invariantes en pruebas unitarias.
- Expón contratos (API, eventos) con OpenAPI/AsyncAPI y versiona cualquier cambio.
- Emite logs estructurados con **Serilog** y propaga contexto de trazas (OpenTelemetry).
- Automatiza pipeline de CI/CD con restauración, build, pruebas y análisis de calidad.

## 7. Recursos adicionales

- [Arquitectura del servicio](./ARCHITECTURE.md)
- [Lenguaje ubicuo contextual](./UBIQUITOUS_LANGUAGE.md)
- [Documentación central de eventos](https://eventmesh-lab.github.io/org-docs/services/events-service/)
- [Guía de contexto de negocio](https://eventmesh-lab.github.io/org-docs/guia-contexto-negocio/)
