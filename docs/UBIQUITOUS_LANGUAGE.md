# Documentación: Events Service

Este directorio contiene la documentación técnica específica para el **Events Service**.

## 1. Descripción General

El `events-service` es el microservicio responsable de la **gestión del ciclo de vida completo de eventos**, desde su creación en estado borrador hasta su finalización. Implementa la lógica de negocio del agregado `Evento` según el lenguaje ubicuo de la plataforma.

**Bounded Context:** Gestión de Eventos

## 2. Documentación Central

La documentación canónica y más detallada de este servicio se encuentra en el repositorio central de `org-docs`. Es fundamental consultarla como fuente principal de verdad.

- **[Ficha del Servicio: Events Service](https://eventmesh-lab.github.io/org-docs/services/events-service/)**

## 3. Documentos Específicos

- **[Arquitectura del Servicio](./ARCHITECTURE.md):** Detalles sobre la implementación de la Arquitectura Hexagonal y DDD en este microservicio.
- **[Lenguaje Ubicuo](./UBIQUITOUS_LANGUAGE.md):** Glosario de términos del dominio relevantes para el contexto de "Gestión de Eventos".
- **[Guía de Desarrollo](./DEVELOPMENT_GUIDE.md):** Instrucciones para preparar el entorno local, ejecutar la API y cubrir la batería de pruebas.

---
*Esta documentación complementa la información centralizada y está destinada a guiar el desarrollo específico de este microservicio.*
