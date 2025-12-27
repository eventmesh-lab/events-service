using events_service.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllers();
builder.Services.ConfigureSwagger();
builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

// Configurar pipeline HTTP
app.ConfigureMiddleware();

app.Run();
