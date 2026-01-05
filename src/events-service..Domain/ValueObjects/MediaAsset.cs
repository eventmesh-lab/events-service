using System;

namespace events_service.Domain.ValueObjects
{
    /// <summary>
    /// Representa un blob asociado a un evento (imagen o folleto).
    /// </summary>
    public sealed record MediaAsset(string BlobName, string ContentType, long SizeBytes)
    {
        public const long MaxSizeBytes = 1_000_000; // ~1 MB

        public static readonly string[] AllowedImageContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        public static readonly string[] AllowedPdfContentTypes =
        {
            "application/pdf"
        };

        public bool EsImagen => Array.Exists(AllowedImageContentTypes, ct => ct.Equals(ContentType, StringComparison.OrdinalIgnoreCase));

        public bool EsPdf => Array.Exists(AllowedPdfContentTypes, ct => ct.Equals(ContentType, StringComparison.OrdinalIgnoreCase));

        public static MediaAsset CrearImagen(string blobName, string contentType, long sizeBytes)
        {
            if (!EsContentTypeImagen(contentType))
                throw new ArgumentException("Tipo de contenido de imagen no soportado.", nameof(contentType));

            AsegurarTamano(sizeBytes);
            return new MediaAsset(blobName, contentType, sizeBytes);
        }

        public static MediaAsset CrearPdf(string blobName, string contentType, long sizeBytes)
        {
            if (!EsContentTypePdf(contentType))
                throw new ArgumentException("El folleto debe ser PDF.", nameof(contentType));

            AsegurarTamano(sizeBytes);
            return new MediaAsset(blobName, contentType, sizeBytes);
        }

        private static void AsegurarTamano(long sizeBytes)
        {
            if (sizeBytes <= 0)
                throw new ArgumentException("El archivo debe tener contenido.", nameof(sizeBytes));

            if (sizeBytes > MaxSizeBytes)
                throw new ArgumentException("El archivo excede el límite de 1MB.", nameof(sizeBytes));
        }

        private static bool EsContentTypeImagen(string contentType) =>
            Array.Exists(AllowedImageContentTypes, ct => ct.Equals(contentType, StringComparison.OrdinalIgnoreCase));

        private static bool EsContentTypePdf(string contentType) =>
            Array.Exists(AllowedPdfContentTypes, ct => ct.Equals(contentType, StringComparison.OrdinalIgnoreCase));
    }
}
