#!/bin/bash
# ============================================
# Script para cargar variables del .env y desplegar
# ============================================

set -e

# Cargar variables del archivo .env
if [ -f .env ]; then
    export $(cat .env | grep -v '#' | xargs)
    echo "✓ Variables cargadas desde .env"
else
    echo "✗ Error: No se encontró archivo .env"
    echo "  Copia .env.example a .env y llena las variables"
    exit 1
fi

# Verificar variables requeridas
required_vars=(
    "ACR_LOGIN_SERVER"
    "ACR_USERNAME"
    "ACR_PASSWORD"
    "AZURE_RESOURCE_GROUP"
)

for var in "${required_vars[@]}"; do
    if [ -z "${!var}" ]; then
        echo "✗ Error: Variable $var no está definida en .env"
        exit 1
    fi
done

echo "================================================"
echo "Configuración para Azure Container Instances"
echo "================================================"
echo "ACR: $ACR_LOGIN_SERVER"
echo "Grupo de recursos: $AZURE_RESOURCE_GROUP"
echo "Región: $AZURE_REGION"
echo ""

# Construir y subir imagen
echo "📦 Construyendo imagen Docker..."
docker build -t events-api:latest .

echo "🏷️  Etiquetando imagen para ACR..."
docker tag events-api:latest $ACR_LOGIN_SERVER/events-api:latest

echo "📤 Subiendo imagen a Azure Container Registry..."
docker push $ACR_LOGIN_SERVER/events-api:latest

echo "✓ Imagen subida exitosamente"
echo ""

# Actualizar credenciales en deploy-aci.yaml
echo "🔐 Actualizando credenciales en deploy-aci.yaml..."
sed -i "s|server:.*|server: $ACR_LOGIN_SERVER|g" deploy-aci.yaml
sed -i "s|username:.*|username: $ACR_USERNAME|g" deploy-aci.yaml
sed -i "s|password:.*|password: $ACR_PASSWORD|g" deploy-aci.yaml
sed -i "s|image:.*events-api|image: $ACR_LOGIN_SERVER/events-api|g" deploy-aci.yaml

# Desplegar en Azure Container Instances
echo "🚀 Desplegando en Azure Container Instances..."
az container create \
    --resource-group $AZURE_RESOURCE_GROUP \
    --file deploy-aci.yaml

echo ""
echo "✓ Despliegue completado"
echo ""

# Obtener IP pública
IP=$(az container show \
    --resource-group $AZURE_RESOURCE_GROUP \
    --name events-container-group \
    --query "properties.ipAddress.ip" -o tsv)

echo "================================================"
echo "Aplicación disponible en:"
echo "  Swagger: http://$IP:8080/swagger/index.html"
echo "  API: http://$IP:8080"
echo "  RabbitMQ: http://$IP:15672"
echo "================================================"
