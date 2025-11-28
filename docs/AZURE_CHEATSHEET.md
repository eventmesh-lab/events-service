# Azure Deployment - Referencia Rápida

## Comandos esenciales

### Autenticación
```bash
az login                                    # Iniciar sesión
az account show --output table              # Ver cuenta activa
```

### Grupo de recursos
```bash
az group create --name proyectoDs20252 --location eastus
az group list --output table
```

### Azure Container Registry
```bash
# Crear ACR
az acr create --resource-group proyectoDs20252 --name acreventproyectods --sku Basic

# Habilitar admin
az acr update --name acreventproyectods --admin-enabled true

# Obtener credenciales
az acr credential show --name acreventproyectods --query '{username: username, password: passwords[0].value}' -o json

# Listar imágenes
az acr repository list --name acreventproyectods --output table
```

### Docker (local)
```bash
# Login
az acr login --name acreventproyectods

# Construir
docker build -t events-api:latest .

# Etiquetar
docker tag events-api:latest acreventproyectods.azurecr.io/events-api:latest

# Subir
docker push acreventproyectods.azurecr.io/events-api:latest
```

### Azure Container Instances
```bash
# Desplegar
az container create --resource-group proyectoDs20252 --file deploy-aci.yaml

# Ver estado
az container show --resource-group proyectoDs20252 --name events-container-group --output table

# Obtener IP
az container show --resource-group proyectoDs20252 --name events-container-group | grep -A 5 '"ip"'

# Ver logs
az container logs --resource-group proyectoDs20252 --name events-container-group --container-name events-api

# Logs en tiempo real
az container logs --resource-group proyectoDs20252 --name events-container-group --container-name events-api --follow

# Reiniciar
az container restart --resource-group proyectoDs20252 --name events-container-group

# Eliminar
az container delete --resource-group proyectoDs20252 --name events-container-group --yes
```

## URLs de acceso

| Servicio | URL |
|----------|-----|
| Swagger | `http://<IP>:8080/swagger/index.html` |
| API Base | `http://<IP>:8080` |
| RabbitMQ Management | `http://<IP>:15672` (guest/guest123) |
| PostgreSQL | `<IP>:5432` |

## Flujo completo de despliegue

```bash
# 1. Autenticarse
az login

# 2. Construir imagen local
docker build -t events-api:latest .

# 3. Etiquetar para ACR
docker tag events-api:latest acreventproyectods.azurecr.io/events-api:latest

# 4. Subir a ACR
az acr login --name acreventproyectods
docker push acreventproyectods.azurecr.io/events-api:latest

# 5. Desplegar en ACI
az container create --resource-group proyectoDs20252 --file deploy-aci.yaml

# 6. Obtener IP
az container show --resource-group proyectoDs20252 --name events-container-group --query "properties.ipAddress.ip" -o tsv
```

## Credenciales por defecto

| Servicio | Usuario | Contraseña |
|----------|---------|-----------|
| PostgreSQL | events | events123 |
| RabbitMQ | guest | guest123 |

## Archivos clave

| Archivo | Propósito |
|---------|----------|
| `Dockerfile` | Definición de imagen Docker |
| `deploy-aci.yaml` | Configuración de despliegue en ACI |
| `.env` | Variables de entorno locales (NO incluir en git) |
| `.env.example` | Plantilla de variables de entorno |
| `deploy.sh` | Script automatizado de despliegue |
| `docs/DEPLOYMENT_GUIDE.md` | Guía completa de despliegue |

## Troubleshooting rápido

| Problema | Solución |
|----------|----------|
| "Name or service not known" | Usar `localhost` en lugar de nombres de contenedor |
| Imagen no encontrada en ACR | Verificar con `az acr repository list` |
| Contenedor no inicia | Ver logs con `az container logs` |
| Puertos no accesibles | Verificar firewall y reglas de seguridad de Azure |
| Datos no persisten | Agregaage Azure Files o Azure Managed Disk |
