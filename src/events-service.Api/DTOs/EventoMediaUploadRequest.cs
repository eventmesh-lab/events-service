using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace events_service.Api.DTOs
{
    /// <summary>
    /// Modelo de request multipart para subir medios de un evento.
    /// </summary>
    public class EventoMediaUploadRequest
    {
        public IFormFile? ImagenPrincipal { get; set; }

        public List<IFormFile> ImagenesSecundarias { get; set; } = new();

        public IFormFile? FolletoPdf { get; set; }
    }
}
