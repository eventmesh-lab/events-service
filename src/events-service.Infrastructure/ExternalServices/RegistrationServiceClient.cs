using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using events_service.Domain.Ports;

namespace events_service.Infrastructure.ExternalServices;

public class RegistrationServiceClient : IRegistrationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RegistrationServiceClient> _logger;

    public RegistrationServiceClient(HttpClient httpClient, ILogger<RegistrationServiceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> GetRegistrationCountAsync(Guid eventId, CancellationToken ct = default)
    {
        try
        {
            // Endpoint: GET /api/registrations/count?eventId={id}
            var response = await _httpClient.GetAsync($"api/registrations/count?eventId={eventId}", ct);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RegistrationCountResponse>(cancellationToken: ct);
                return result?.Count ?? 0;
            }

            _logger.LogWarning("Failed to get registration count for event {EventId}. Status: {StatusCode}", eventId, response.StatusCode);
            return 0; // Or throw depending on how critical this is. For deletion, 0 is safer but risky if service is down.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Registration Service for event {EventId}", eventId);
            return 0;
        }
    }

    private record RegistrationCountResponse(int Count);
}
