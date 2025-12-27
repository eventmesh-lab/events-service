using System.IO;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace events_service.Api.Configuration;

/// <summary>
/// Configuración de Swagger/OpenAPI.
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Configura Swagger para la aplicación.
    /// </summary>
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Events Service API",
                Version = "v1",
                Description = "API para la gestión del ciclo de vida completo de eventos. " +
                              "Permite crear, editar, publicar y gestionar eventos desde su creación en estado borrador hasta su finalización. " +
                              "Incluye gestión de secciones, precios y estados del evento.",
                Contact = new OpenApiContact
                {
                    Name = "Events Service Team",
                    Email = "events-service@eventmesh-lab.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Incluir comentarios XML si existen
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Configurar schemaIds únicos para evitar conflictos con tipos anidados con el mismo nombre
            // Esto resuelve el problema cuando múltiples comandos tienen clases anidadas con el mismo nombre (ej: SeccionDto)
            c.CustomSchemaIds(type =>
            {
                // Para tipos anidados (que contienen '+'), reemplazamos el '+' con el nombre del tipo contenedor
                // Ejemplo: "CrearEventoCommand+SeccionDto" -> "CrearEventoCommandSeccionDto"
                if (type.FullName != null && type.FullName.Contains('+'))
                {
                    var parts = type.FullName.Split('+');
                    var containingType = parts[0].Split('.').Last(); // Obtener solo el nombre del tipo contenedor
                    var nestedType = parts[1].Split('.').Last(); // Obtener solo el nombre del tipo anidado
                    return $"{containingType}{nestedType}";
                }

                // Para tipos normales, usar solo el nombre del tipo (sin namespace)
                return type.Name;
            });
        });

        return services;
    }
}
