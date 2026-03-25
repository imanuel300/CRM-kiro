using System.Net.Http.Json;
using CandidacyManagement.Application.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CandidacyManagement.Infrastructure.Sms;

/// <summary>
/// שירות שליחת SMS באמצעות ספק SMS חיצוני (HTTP API)
/// </summary>
public class SmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SmsService> _logger;

    public SmsService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<SmsService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<NotificationDeliveryResult> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiUrl = _configuration["Sms:ApiUrl"];
            var apiKey = _configuration["Sms:ApiKey"] ?? string.Empty;
            var senderName = _configuration["Sms:SenderName"] ?? "CandidacyMgmt";

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                _logger.LogWarning("SMS API URL is not configured. SMS to {PhoneNumber} was not sent.", phoneNumber);
                return new NotificationDeliveryResult(false, "SMS provider is not configured");
            }

            var client = _httpClientFactory.CreateClient("SmsProvider");

            var payload = new
            {
                to = phoneNumber,
                from = senderName,
                text = message
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = JsonContent.Create(payload);

            var response = await client.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
                return new NotificationDeliveryResult(true);
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to send SMS to {PhoneNumber}. Status: {StatusCode}, Body: {Body}",
                phoneNumber, response.StatusCode, errorBody);

            return new NotificationDeliveryResult(false, $"SMS provider returned {(int)response.StatusCode}: {errorBody}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return new NotificationDeliveryResult(false, ex.Message);
        }
    }
}
