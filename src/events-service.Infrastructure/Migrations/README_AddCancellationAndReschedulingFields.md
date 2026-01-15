# Database Migration: AddCancellationAndReschedulingFields

This migration adds new columns to the `eventos` table to support event cancellation metadata and rescheduling history.

## New Columns Added

| Column Name | Type | Description |
| ----------- | ---- | ----------- |
| `motivo_cancelacion` | `text` | Reason for event cancellation. |
| `fecha_cancelacion` | `timestamp with time zone` | When the event was cancelled. |
| `cancelado_por` | `character varying(100)` | User ID who performed the cancellation. |
| `fecha_inicio_original` | `timestamp with time zone` | Original start date before the first rescheduling. |
| `fecha_fin_original` | `timestamp with time zone` | Original end date before the first rescheduling. |
| `contador_reprogramaciones` | `integer` | Number of times the event has been rescheduled (Default: 0). |
| `ultima_reprogramacion_fecha` | `timestamp with time zone` | Last time the event dates were modified. |
| `ultima_reprogramacion_por` | `character varying(100)` | User ID who performed the last rescheduling. |

## How to Apply

To apply this migration to your local database, run:

```bash
dotnet ef database update --project src/events-service.Infrastructure/events-service.Infrastructure.csproj --startup-project src/events-service.Api/events-service.Api.csproj
```

## Rollback

To remove this migration:

```bash
dotnet ef database update <PreviousMigrationName> --project src/events-service.Infrastructure/events-service.Infrastructure.csproj --startup-project src/events-service.Api/events-service.Api.csproj
```
