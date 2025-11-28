# Guía de Despliegue en Azure Container Instances

Guía paso a paso para desplegar la aplicación Events Service en Azure Container Instances (ACI).

## Requisitos previos

- Azure CLI instalado (`az` command)
- Docker instalado
- Cuenta de Azure activa
- Git configurado

## Pasos generales de despliegue

### 1. Autenticarse en Azure

Inicia sesión en tu cuenta de Azure:

```bash
az login
```

Se abrirá el navegador para completar la autenticación. Después de completar, verás tu suscripción activa.

**Verificar la suscripción activa:**
```bash
az account show --output table
```

### 2. Crear un grupo de recursos (si no existe)

Un grupo de recursos es un contenedor para todos tus recursos en Azure:

```bash
az group create \
  --name proyectoDs20252 \
  --location eastus
```

**Listar grupos de recursos:**
```bash
az group list --output table
```

### 3. Crear un Azure Container Registry (ACR)

ACR es donde almacenarás tus imágenes Docker privadas:

```bash
az acr create \
  --resource-group proyectoDs20252 \
  --name acreventproyectods \
  --sku Basic
```

> **Nota:** El nombre del ACR debe ser único y alfanumérico (sin guiones)

**Verificar que se creó correctamente:**
```bash
az acr list --resource-group proyectoDs20252 --output table
```

### 4. Habilitar acceso administrativo al ACR

Esto permite que Azure Container Instances acceda a tus imágenes:

```bash
az acr update \
  --name acreventproyectods \
  --admin-enabled true
```

### 5. Obtener las credenciales del ACR

Necesitarás el usuario y contraseña para el YAML de despliegue:

```bash
az acr credential show \
  --name acreventproyectods \
  --query '{username: username, password: passwords[0].value}' \
  -o json
```

**Guardar las credenciales en `.env`:**

Copia los valores obtenidos a tu archivo `.env`:

```bash
ACR_LOGIN_SERVER=acreventproyectods.azurecr.io
ACR_USERNAME=acreventproyectods
ACR_PASSWORD=<contraseña_del_acr>
```

### 6. Iniciar sesión en el ACR localmente

Esto permite que Docker acceda a tu registro:

```bash
az acr login --name acreventproyectods
```

### 7. Construir la imagen Docker

En el directorio raíz del proyecto:

```bash
docker build -t events-api:latest .
```

**Verificar que se construyó:**
```bash
docker images | grep events-api
```

### 8. Etiquetar la imagen para tu ACR

```bash
docker tag events-api:latest acreventproyectods.azurecr.io/events-api:latest
```

### 9. Subir la imagen a ACR

```bash
docker push acreventproyectods.azurecr.io/events-api:latest
```

**Verificar que se subió:**
```bash
az acr repository list --name acreventproyectods --output table
```

### 10. Preparar las imágenes de dependencias

Sube también PostgreSQL y RabbitMQ a tu ACR:

#### PostgreSQL:
```bash
docker pull postgres:16
docker tag postgres:16 acreventproyectods.azurecr.io/postgres:16
docker push acreventproyectods.azurecr.io/postgres:16
```

#### RabbitMQ:
```bash
docker pull rabbitmq:3.13-alpine
docker tag rabbitmq:3.13-alpine acreventproyectods.azurecr.io/rabbitmq:3.13-alpine
docker push acreventproyectods.azurecr.io/rabbitmq:3.13-alpine
```

### 11. Crear el archivo YAML de despliegue

Crea o actualiza `deploy-aci.yaml` con la configuración correcta. Ejemplo:

```yaml
apiVersion: '2021-07-01'
location: eastus
name: events-container-group
properties:
  containers:
  - name: postgres
    properties:
      image: acreventproyectods.azurecr.io/postgres:16
      resources:
        requests:
          cpu: 0.5
          memoryInGb: 1.0
      environmentVariables:
      - name: POSTGRES_DB
        value: events
      - name: POSTGRES_USER
        value: events
      - name: POSTGRES_PASSWORD
        secureValue: events123
      ports:
      - port: 5432
        protocol: TCP

  - name: rabbitmq
    properties:
      image: acreventproyectods.azurecr.io/rabbitmq:3.13-alpine
      resources:
        requests:
          cpu: 0.5
          memoryInGb: 1.0
      environmentVariables:
      - name: RABBITMQ_DEFAULT_USER
        value: guest
      - name: RABBITMQ_DEFAULT_PASS
        secureValue: guest123
      ports:
      - port: 5672
        protocol: TCP
      - port: 15672
        protocol: TCP

  - name: events-api
    properties:
      image: acreventproyectods.azurecr.io/events-api:latest
      resources:
        requests:
          cpu: 1.0
          memoryInGb: 1.0
      environmentVariables:
      - name: ASPNETCORE_ENVIRONMENT
        value: Development
      - name: ConnectionStrings__EventsDb
        value: Host=localhost;Port=5432;Database=events;Username=events;Password=events123
      - name: MessageBroker__Host
        value: localhost
      - name: MessageBroker__Exchange
        value: eventos.domain.events
      - name: MessageBroker__Username
        value: guest
      - name: MessageBroker__Password
        value: guest123
      ports:
      - port: 8080
        protocol: TCP

  osType: Linux
  restartPolicy: Always
  imageRegistryCredentials:
  - server: acreventproyectods.azurecr.io
    username: acreventproyectods
    password: <CONTRASEÑA_ACR>

  ipAddress:
    type: Public
    dnsNameLabel: events-service-api
    ports:
    - port: 8080
      protocol: TCP
    - port: 5432
      protocol: TCP
    - port: 5672
      protocol: TCP
    - port: 15672
      protocol: TCP
```

### 12. Desplegar en Azure Container Instances

Crea el grupo de contenedores:

```bash
az container create \
  --resource-group proyectoDs20252 \
  --file deploy-aci.yaml
```

### 13. Verificar el estado del despliegue

Espera unos segundos y verifica que los contenedores están corriendo:

```bash
az container show \
  --resource-group proyectoDs20252 \
  --name events-container-group \
  --output table
```

### 14. Obtener la IP pública

```bash
az container show \
  --resource-group proyectoDs20252 \
  --name events-container-group \
  --query "properties.ipAddress.ip" \
  -o tsv
```

O con más detalles:

```bash
az container show \
  --resource-group proyectoDs20252 \
  --name events-container-group | grep -A 5 '"ip"'
```

### 15. Acceder a la aplicación

Una vez obtenida la IP (ej: `20.242.179.111`), accede a:

- **Swagger API**: `http://20.242.179.111:8080/swagger/index.html`
- **API Base**: `http://20.242.179.111:8080`
- **RabbitMQ Management**: `http://20.242.179.111:15672` (usuario: guest, contraseña: guest123)
- **PostgreSQL**: `20.242.179.111:5432`

O usando el nombre DNS:

- **Swagger API**: `http://events-service-api.eastus.azurecontainer.io:8080/swagger/index.html`

## Monitoreo y diagnóstico

### Ver logs de un contenedor específico

```bash
# Logs de la API
az container logs \
  --resource-group proyectoDs20252 \
  --name events-container-group \
  --container-name events-api

# Logs de PostgreSQL
az container logs \
  --resource-group proyectoDs20252 \
  --name events-container-group \
  --container-name postgres

# Logs de RabbitMQ
az container logs \
  --resource-group proyectoDs20252 \
  --name events-container-group \
  --container-name rabbitmq
```

### Ver logs en tiempo real

```bash
az container logs \
  --resource-group proyectoDs20252 \
  --name events-container-group \
  --container-name events-api \
  --follow
```

## Limpieza de recursos

### Eliminar el grupo de contenedores

```bash
az container delete \
  --resource-group proyectoDs20252 \
  --name events-container-group \
  --yes
```

### Eliminar el grupo de recursos completo

```bash
az group delete \
  --name proyectoDs20252 \
  --yes
```

## Script automatizado

Para simplificar el proceso, usa el script `deploy.sh`:

```bash
chmod +x deploy.sh
./deploy.sh
```

El script automatiza los pasos 7-14.

## Solución de problemas

### Error: "Name or service not known"
**Causa:** Los contenedores no pueden resolver los nombres de otros contenedores.
**Solución:** Usar `localhost` en lugar de nombres de contenedor en las cadenas de conexión.

### Error: "Registry not found"
**Causa:** La imagen no está en el ACR.
**Solución:** Verificar que la imagen se subió correctamente con `az acr repository list`.

### Error: "Broker unreachable"
**Causa:** La API no puede conectarse a RabbitMQ durante el startup.
**Solución:** Usar lazy loading en la conexión de RabbitMQ (ya implementado).

### Aplicación no responde
**Solución:** 
1. Verificar logs: `az container logs --resource-group proyectoDs20252 --name events-container-group --container-name events-api`
2. Verificar estado: `az container show --resource-group proyectoDs20252 --name events-container-group --output table`
3. Reiniciar: `az container restart --resource-group proyectoDs20252 --name events-container-group`

## Variables de entorno importantes

| Variable | Valor |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Development` o `Production` |
| `ConnectionStrings__EventsDb` | `Host=localhost;Port=5432;Database=events;Username=events;Password=events123` |
| `MessageBroker__Host` | `localhost` |
| `MessageBroker__Exchange` | `eventos.domain.events` |
| `POSTGRES_DB` | `events` |
| `POSTGRES_USER` | `events` |
| `POSTGRES_PASSWORD` | `events123` |
| `RABBITMQ_DEFAULT_USER` | `guest` |
| `RABBITMQ_DEFAULT_PASS` | `guest123` |

## Referencias

- [Microsoft Learn - Azure Container Instances](https://learn.microsoft.com/es-es/azure/container-instances/)
- [Documentación YAML para ACI](https://learn.microsoft.com/es-es/azure/container-instances/container-instances-reference-yaml)
- [Azure CLI - Contenedores](https://learn.microsoft.com/es-es/cli/azure/container)
