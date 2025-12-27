namespace events_service.Api.Configuration;

/// <summary>
/// Configuración del pipeline de middleware HTTP.
/// </summary>
public static class MiddlewareConfiguration
{
    /// <summary>
    /// Configura el pipeline de middleware HTTP.
    /// </summary>
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        // Configurar pipeline HTTP
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Events Service API v1");
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseCors("AllowFrontend");

        // Mapear controladores
        app.MapControllers();

        return app;
    }
}
