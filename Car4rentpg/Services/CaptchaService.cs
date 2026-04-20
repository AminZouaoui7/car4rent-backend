using System.Text.Json;
using Car4rentpg.Models;
using Microsoft.Extensions.Options;

namespace Car4rentpg.Services
{
    public class CaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly CaptchaSettings _settings;
        private readonly ILogger<CaptchaService> _logger;

        public CaptchaService(
            HttpClient httpClient,
            IOptions<CaptchaSettings> options,
            ILogger<CaptchaService> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<CaptchaVerificationResult> VerifyAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Captcha token is missing.");
                return new CaptchaVerificationResult
                {
                    Success = false,
                    ErrorMessage = "Captcha token is missing."
                };
            }

            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            {
                _logger.LogError("Captcha secret key is not configured.");
                return new CaptchaVerificationResult
                {
                    Success = false,
                    ErrorMessage = "Captcha secret key is not configured."
                };
            }

            var form = new Dictionary<string, string>
            {
                ["secret"] = _settings.SecretKey,
                ["response"] = token
            };

            using var content = new FormUrlEncodedContent(form);
            using var response = await _httpClient.PostAsync(
                "https://challenges.cloudflare.com/turnstile/v0/siteverify",
                content);

            var json = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Turnstile raw response: {Response}", json);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Captcha verification request failed with status code {StatusCode}", response.StatusCode);
                return new CaptchaVerificationResult
                {
                    Success = false,
                    ErrorMessage = "Captcha verification request failed."
                };
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var success = root.TryGetProperty("success", out var successProp) && successProp.GetBoolean();

            if (!success)
            {
                string? errorMessage = null;

                if (root.TryGetProperty("error-codes", out var errorsProp) &&
                    errorsProp.ValueKind == JsonValueKind.Array)
                {
                    var errors = errorsProp.EnumerateArray()
                        .Select(x => x.GetString())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();

                    errorMessage = string.Join(", ", errors!);
                    _logger.LogWarning("Turnstile verification failed. Error codes: {ErrorCodes}", errorMessage);
                }
                else
                {
                    _logger.LogWarning("Turnstile verification failed without explicit error-codes.");
                }

                return new CaptchaVerificationResult
                {
                    Success = false,
                    ErrorMessage = errorMessage ?? "Captcha verification failed."
                };
            }

            _logger.LogInformation("Turnstile verification succeeded.");
            return new CaptchaVerificationResult
            {
                Success = true
            };
        }
    }
}