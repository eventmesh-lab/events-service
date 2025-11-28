# Guía de Despliegue en Azure Container Instances

## Requisitos Previos
- Azure CLI instalado (`az --version`)
- Acceso a una suscripción de Azure
- `docker-compose.yml` configurado en la raíz del proyecto

## Pasos de Despliegue

### 1. Autenticación en Azure

```bash
# Verificar si ya está autenticado
az account show 2>&1 | head -20

# Si no está autenticado, ejecutar:
az login
```

### 2. Definir Variables de Entorno

```bash
# Configurar variables para reutilizar
RESOURCE_GROUP="Proyecto_DS_2025-02"
STORAGE_ACCOUNT="eventos2025storage"
LOCATION="eastus"
CONTAINER_GROUP_NAME="eventos-service-group"
DNS_LABEL="eventos2025"
```

### 3. Crear Grupo de Recursos

```bash
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

### 4. Crear Cuenta de Almacenamiento

```bash
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS
```

### 5. Obtener Clave de Almacenamiento

```bash
STORAGE_KEY=$(az storage account keys list \
  --resource-group $RESOURCE_GROUP \
  --account-name $STORAGE_ACCOUNT \
  --query "[0].value" -o tsv)

echo "Clave de almacenamiento: $STORAGE_KEY"
```

### 6. Crear Compartimentos de Almacenamiento (File Shares)

```bash
# Crear compartimentos para persistencia de datos
az storage share create \
  --account-name $STORAGE_ACCOUNT \
  --name postgres-data

az storage share create \
  --account-name $STORAGE_ACCOUNT \
  --name rabbitmq-data

az storage share create \
  --account-name $STORAGE_ACCOUNT \
  --name api-data
```

### 7. Crear Archivo YAML de Contenedores

Crear `azure-container-instances.yaml` con la configuración de tus contenedores:

```yaml
apiVersion: '2021-07-01'
location: eastus
name: eventos-service-group
type: Microsoft.ContainerInstance/containerGroups
properties:
  containers:
  - name: postgres
    properties:
      image: postgres:16-alpine
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
        secureValue: events
      volumeMounts:
      - name: postgres-data
        mountPath: /var/lib/postgresql/data
      ports:
      - port: 5432
        protocol: TCP

  - name: rabbitmq
    properties:
      image: rabbitmq:3.13-alpine
      resources:
        requests:
          cpu: 0.5
          memoryInGb: 1.0
      environmentVariables:
      - name: RABBITMQ_DEFAULT_USER
        value: guest
      - name: RABBITMQ_DEFAULT_PASS
        secureValue: guest
      volumeMounts:
      - name: rabbitmq-data
        mountPath: /var/lib/rabbitmq
      ports:
      - port: 5672
        protocol: TCP
      - port: 15672
        protocol: TCP

  - name: api
    properties:
      image: your-registry.azurecr.io/events-api:latest
      resources:
        requests:
          cpu: 1.0
          memoryInGb: 1.5
      environmentVariables:
      - name: ASPNETCORE_ENVIRONMENT
        value: Production
      - name: ConnectionStrings__EventsDb
        secureValue: Host=localhost;Port=5432;Database=events;Username=events;Password=events
      - name: MessageBroker__Host
        value: localhost
      - name: MessageBroker__Exchange
        value: eventos.domain.events
      - name: MessageBroker__Username
        value: guest
      - name: MessageBroker__Password
        secureValue: guest
      volumeMounts:
      - name: api-data
        mountPath: /app/data
      ports:
      - port: 8080
        protocol: TCP

  osType: Linux
  restartPolicy: Always
  ipAddress:
    type: Public
    dnsNameLabel: eventos2025
    ports:
    - port: 5432
      protocol: TCP
    - port: 5672
      protocol: TCP
    - port: 15672
      protocol: TCP
    - port: 8080
      protocol: TCP

  volumes:
  - name: postgres-data
    azureFile:
      shareName: postgres-data
      storageAccountName: eventos2025storage
      storageAccountKey: YOUR_STORAGE_KEY_HERE

  - name: rabbitmq-data
    azureFile:
      shareName: rabbitmq-data
      storageAccountName: eventos2025storage
      storageAccountKey: YOUR_STORAGE_KEY_HERE

  - name: api-data
    azureFile:
      shareName: api-data
      storageAccountName: eventos2025storage
      storageAccountKey: YOUR_STORAGE_KEY_HERE
```

### 8. Desplegar Grupo de Contenedores

```bash
# Reemplazar la clave de almacenamiento en el YAML
sed -i "s|YOUR_STORAGE_KEY_HERE|$STORAGE_KEY|g" azure-container-instances.yaml

# Desplegar
az container create \
  --resource-group $RESOURCE_GROUP \
  --file azure-container-instances.yaml
```

### 9. Verificar Despliegue

```bash
# Ver estado del grupo de contenedores
az container show \
  --resource-group $RESOURCE_GROUP \
  --name eventos-service-group \
  --query "{State:instanceView.state, IP:ipAddress.ip, FQDN:ipAddress.fqdn}"

# Ver logs de un contenedor específico
az container logs \
  --resource-group $RESOURCE_GROUP \
  --name eventos-service-group \
  --container-name api

# Ver logs de todos los contenedores
az container logs \
  --resource-group $RESOURCE_GROUP \
  --name eventos-service-group
```

## Puntos Importantes

### Para Contenedores Linux
✅ Soportan volúmenes de Azure Files
✅ Imágenes ligeras (alpine)
✅ Más eficientes en costos


### Variables de Entorno Sensibles
- Usar `secureValue` en el YAML para contraseñas
- No comprometer claves en control de versiones
- Usar Azure Key Vault para mayor seguridad

### Registros de Contenedores
- **Docker Hub**: Puede tener problemas de disponibilidad
- **MCR** (Microsoft Container Registry): Recomendado para Azure
- **ACR** (Azure Container Registry): Ideal para imágenes privadas

## Limpiar Recursos

```bash
# Eliminar grupo de contenedores
az container delete \
  --resource-group $RESOURCE_GROUP \
  --name eventos-service-group \
  --yes

# Eliminar cuenta de almacenamiento
az storage account delete \
  --resource-group $RESOURCE_GROUP \
  --name $STORAGE_ACCOUNT \
  --yes

# Eliminar grupo de recursos (elimina todo)
az group delete \
  --name $RESOURCE_GROUP \
  --yes
```

## Troubleshooting

### Error: RegistryErrorResponse
**Causa**: Problema con Docker Hub  
**Solución**: Usar imágenes de MCR o esperar a que se recupere

### Error: WindowsContainersVolumeNotSupported
**Causa**: Intentar usar volúmenes con contenedores Windows  
**Solución**: Usar contenedores Linux o Azure Blob Storage

### Error: ResourceGroupNotFound
**Causa**: El grupo de recursos no existe  
**Solución**: Crear el grupo de recursos con `az group create`

### Contenedores No Inician
**Diagnóstico**:
```bash
az container logs \
  --resource-group $RESOURCE_GROUP \
  --name eventos-service-group
```

## Referencias
- [Azure Container Instances Documentation](https://learn.microsoft.com/en-us/azure/container-instances/)
- [Azure CLI Reference](https://learn.microsoft.com/en-us/cli/azure/reference-index)
- [Container YAML Reference](https://learn.microsoft.com/en-us/azure/container-instances/container-instances-reference-yaml)
