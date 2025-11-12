# events-service

Este repositorio contiene un microservicio .NET organizado con la intención de seguir la arquitectura hexagonal (Ports & Adapters). El README original era una plantilla; este archivo describe lo que realmente hay en este repo y, en particular, documenta el contenido de la carpeta `docs/`.

Estructura principal (resumen):

- `events-service.sln` — solución .NET que agrupa los proyectos.
- `Dockerfile`, `docker-compose.yml` — artefactos para contenedores/local orchestration.
- `src/` — código fuente:
  - `events-service.Domain/` — entidades, value objects y lógica de dominio.
  - `events-service.Application/` — casos de uso, servicios de aplicación y comandos.
  - `events-service.Infrastructure/` — repositorios, adaptadores y dependencias externas.
  - `events-service.Api/` — API HTTP (entrada) y Program/Startup.
- `tests/` — pruebas automatizadas:
  - `events-service.Domain.Tests/` — pruebas unitarias de dominio.
  - `events-service.Application.Tests/` — pruebas de los casos de uso/servicios.
  - `events-service.Infrastructure.IntegrationTests/` — pruebas de integración (repositorios, adapters).
- `docs/` — documentación del proyecto (ver sección dedicada abajo).

Docs (carpeta `docs/`) — contenido clave

La carpeta `docs/` contiene la documentación más importante del repositorio. Archivos relevantes:

- `ARCHITECTURE.md` — descripción de la arquitectura (capas, dependencias entre proyectos, decisiones arquitectónicas y diagramas o referencias cuando aplican).
- `DEVELOPMENT_GUIDE.md` — guía para desarrolladores: cómo configurar el entorno local, convención de ramas, formato de commits, flujo de trabajo y cómo añadir nuevas features o adaptadores.
- `TDD_TEST_PLAN.md` — plan y criterios para pruebas basadas en TDD; qué pruebas existen y la estrategia de pruebas (unitarias vs integración).
- `UBIQUITOUS_LANGUAGE.md` — glosario y términos del dominio (vocabulario compartido entre negocio y equipo técnico).

Recomendación: revise `docs/` antes de desarrollar; ahí está la intención del diseño y las reglas del repositorio.

Cómo compilar, ejecutar y probar (comandos básicos)

Los comandos siguientes asumen que tienes el SDK de .NET instalado (recomendado: .NET 8 o la versión que figura en los `.csproj`). Ejecuta estos comandos desde la raíz del repositorio usando tu shell (el proyecto está en Windows, pero los comandos son los estándar de dotnet):

```bash
# Restaurar paquetes
dotnet restore

# Compilar la solución
dotnet build ./events-service.sln

# Ejecutar la API localmente (carpeta con Program.cs)
dotnet run --project ./src/events-service.Api/events-service.Api.csproj

# Ejecutar todos los tests
dotnet test ./tests/events-service.Domain.Tests/events-service.Domain.Tests.csproj
dotnet test ./tests/events-service.Application.Tests/events-service.Application.Tests.csproj
# Tests de integración (si correspondiera)
dotnet test ./tests/events-service.Infrastructure.IntegrationTests/events-service.Infrastructure.IntegrationTests.csproj
```

Otras notas útiles

- La solución `events-service.sln` agrupa los proyectos; puedes abrirla en Visual Studio o VS Code (con extensiones C#).
- Para ejecutar en contenedor localmente, consulta `docker-compose.yml` y `Dockerfile`.
- El código sigue la separación de responsabilidades: `Domain` no debe depender de `Infrastructure`.
- Si vas a modificar diseño o contratos (por ejemplo, modelos públicos de la API), actualiza también la documentación en `docs/` y el `UBIQUITOUS_LANGUAGE.md`.

Contribuciones y seguimiento

- Si encuentras errores en la documentación, mejora `docs/*.md` y crea un PR claro que describa el cambio.
- Si vas a añadir una feature o cambiar la arquitectura, abre un issue primero y referencia los documentos en `docs/`.
