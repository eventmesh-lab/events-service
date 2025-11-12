using System;
using System.IO;

namespace events_service.Infrastructure.Fallback
{
    /// <summary>
    /// Opciones de configuración para el almacenamiento de eventos en fallback.
    /// </summary>
    public class EventoFallbackOptions
    {
        /// <summary>
        /// Ruta absoluta del archivo JSON donde se persistirán los eventos de respaldo.
        /// </summary>
        public string? EventsFilePath { get; set; }

        /// <summary>
        /// Obtiene la ruta efectiva garantizando un valor predeterminado cuando no se configura.
        /// </summary>
        public string ResolvePath()
        {
            if (!string.IsNullOrWhiteSpace(EventsFilePath))
            {
                return EventsFilePath!;
            }

            var appData = Path.Combine(AppContext.BaseDirectory, "App_Data");
            return Path.Combine(appData, "events-fallback.json");
        }
    }
}
