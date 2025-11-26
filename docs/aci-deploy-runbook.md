# Despliegue de `events-service` en Azure Container Instances

Esta bitácora resume los pasos que ejecutamos (con comandos reales) para pasar de un entorno local con Docker Compose a un despliegue completo en Azure Container Instances (ACI) usando un Azure Container Registry (ACR). Incluye los errores relevantes que aparecieron y cómo los resolvimos.

## 1. Autenticación e inicio

```powershell
az login --use-device-code
```

> Resultado: autenticación correcta con la suscripción donde creamos todo.

## 2. Preparar variables y `.env.aci-demo`

- Archivo creado: `Services/events-service/.env.aci-demo`. **Nunca** subas valores sensibles reales al repositorio.
- Contiene llaves como `RG_NAME`, `ACR_NAME`, `ACR_LOGIN_SERVER`, `ACR_USERNAME`, `ACR_PASSWORD`, `POSTGRES_*`, etc. Edita el archivo y rellena los valores que correspondan a tu entorno (la contraseña puedes obtenerla con `az acr credential show ...` y luego pegarla en el `.env`).

Cargar las variables en PowerShell (desde `Services/events-service`):

```powershell
Get-Content .env.aci-demo |
  Where-Object { $_ -and $_ -notmatch '^#' } |
  ForEach-Object {
   $name,$value = $_.Split('=',2)
   Set-Item -Path "env:$name" -Value $value
  }

$RG    = $env:RG_NAME
$LOC   = $env:AZURE_LOCATION
$ACR   = $env:ACR_NAME
$LOGIN = $env:ACR_LOGIN_SERVER
$USER  = $env:ACR_USERNAME
$PASS  = $env:ACR_PASSWORD
```

> Si prefieres no cargar variables en el entorno global, puedes convertir ese bloque en un script `.ps1` y ejecutarlo con `& .\load-env.ps1`.

## 3. Creación de recursos base

1. Resource Group
  ```powershell
  az group create -n $RG -l $LOC
  ```

2. Azure Container Registry (con admin habilitado para pruebas rápidas)
  ```powershell
  az acr create -n $ACR -g $RG --sku Basic --admin-enabled true
  ```

## 4. Construcción y publicación de la imagen del API

Desde `Services/events-service`:

```powershell
docker build -t events-api:local -f Dockerfile .
docker tag events-api:local "$LOGIN/events-api:$env:API_IMAGE_TAG"
az acr login --name $ACR
docker push "$LOGIN/events-api:$env:API_IMAGE_TAG"
```

> Alternativa usada también: `az acr build --registry $ACR --image events-api:$env:API_IMAGE_TAG --file Dockerfile .`

## 5. Sincronizar imágenes base en el ACR (evitar rate limits)

Cuando intentamos crear contenedores directamente desde Docker Hub, ACI devolvió `RegistryErrorResponse` por límites de `index.docker.io`. Importamos las imágenes oficiales al ACR para consumirlas de allí:

```powershell
az acr import -n $ACR --source docker.io/library/postgres:16 --image postgres:16 --force
az acr import -n $ACR --source docker.io/library/rabbitmq:3-management --image rabbitmq:3-management --force
```

## 6. Despliegue en Azure Container Instances

### 5.1 Postgres

```powershell
az container create `
  --resource-group $RG --name events-postgres `
  --os-type Linux `
  --image "$LOGIN/postgres:16" `
  --cpu 1 --memory 1.5 `
  --ports 5432 `
  --environment-variables `
      POSTGRES_DB=$env:POSTGRES_DB `
      POSTGRES_USER=$env:POSTGRES_USER `
      POSTGRES_PASSWORD=$env:POSTGRES_PASSWORD `
  --registry-login-server $LOGIN --registry-username $USER --registry-password $PASS `
  --ip-address Public

$PG_IP = az container show -g $RG -n events-postgres --query "ipAddress.ip" -o tsv
```

### 5.2 RabbitMQ

```powershell
az container create `
  --resource-group $RG --name events-rabbitmq `
  --os-type Linux `
  --image "$LOGIN/rabbitmq:3-management" `
  --cpu 1 --memory 1.5 `
  --ports 5672 15672 `
  --environment-variables `
      RABBITMQ_DEFAULT_USER=$env:RABBITMQ_DEFAULT_USER `
      RABBITMQ_DEFAULT_PASS=$env:RABBITMQ_DEFAULT_PASS `
  --registry-login-server $LOGIN --registry-username $USER --registry-password $PASS `
  --ip-address Public

$RAB_IP = az container show -g $RG -n events-rabbitmq --query "ipAddress.ip" -o tsv
```

### 5.3 API (imagen propia)

```powershell
az container create `
  --resource-group $RG --name events-api `
  --os-type Linux `
  --image "$LOGIN/events-api:latest" `
  --cpu 1 --memory 1.5 `
  --ports 8080 8443 `
  --environment-variables `
      ASPNETCORE_ENVIRONMENT=Development `
      ConnectionStrings__EventsDb="Host=$PG_IP;Port=5432;Database=$env:POSTGRES_DB;Username=$env:POSTGRES_USER;Password=$env:POSTGRES_PASSWORD" `
      MessageBroker__Host=$RAB_IP `
      MessageBroker__Exchange=eventos.domain.events `
      MessageBroker__Username=$env:RABBITMQ_DEFAULT_USER `
      MessageBroker__Password=$env:RABBITMQ_DEFAULT_PASS `
      FallbackStorage__EventsFilePath=/app/data/events-fallback.json `
  --registry-login-server $LOGIN --registry-username $USER --registry-password $PASS `
  --ip-address Public
```

### 5.4 Verificación

```powershell
az container list -g $RG -o table
az container logs -g $RG -n events-api
az container logs -g $RG -n events-postgres
az container logs -g $RG -n events-rabbitmq
```

IPs obtenidas durante la sesión (cambiarán si se recrean):

| Servicio        | IP pública      | Puertos |
|-----------------|-----------------|---------|
| API             | `48.216.150.118`| 8080, 8443 |
| Postgres        | `172.212.60.75` | 5432 |
| RabbitMQ        | `20.246.204.13` | 5672, 15672 |

## 7. Errores destacados y soluciones

| Error | Contexto | Solución |
|-------|----------|----------|
| `(InvalidOsType) The 'osType' for container group '<null>' is invalid` | Primer intento de `az container create` sin especificar SO. | Añadimos `--os-type Linux` en cada comando de ACI. |
| `(ResourceNotFound) containerGroups/... was not found` | Aparecía al intentar consultar un contenedor que falló durante la creación anterior. | Reintentamos el despliegue tras corregir los parámetros (ver error anterior y siguiente). |
| `(RegistryErrorResponse) ... docker registry 'index.docker.io'` | Docker Hub rate limit / throttling mientras ACI descargaba `postgres` y `rabbitmq`. | Importamos las imágenes al ACR (`az acr import ...`) y volvimos a crear los contenedores usando `--registry-login-server` + credenciales. |

## 8. Observaciones finales

- `docker-compose.yml` quedó apuntando a `myacruniqueid.azurecr.io/events-api:latest`, para que todos los entornos consuman la misma imagen publicada.
- El workflow en `.github/workflows/build-and-push-acr.yml` permite automatizar la publicación de la imagen del API con GitHub Actions.
- Para limpiar el entorno demo y evitar costos: `az container delete -g $RG -n <grupo> --yes` y, si ya no se usa, `az group delete -n $RG --yes --no-wait`.