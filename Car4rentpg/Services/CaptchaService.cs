using System.Text.Json;
using Car4rentpg.Models;
using Microsoft.Extensions.Options;

namespace Car4rentpg.Services
{
    public class CaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly CaptchaSettings _settings;

        public CaptchaService(HttpClient httpClient, IOptions<CaptchaSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<CaptchaVerificationResult> VerifyAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new CaptchaVerificationResult
                {
                    Success = false,
                    ErrorMessage = "Captcha token is missing."
                };
            }

            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            {
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
            using var response = await _httpClient.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content);

            if (!response.IsSuccessStatusCode)
            {
                return new CaptchaVerificationResult
                {
                    Success = false,
                    ErrorMessage = "Captcha verification request failed."
                };
            }

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var success = root.TryGetProperty("success", out var successProp) && successProp.GetBoolean();

            if (!success)
            {
                string? errorMessage = null;

                if (root.TryGetProperty("error-codes", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Array)
                {
                    var errors = errorsProp.EnumerateArray()
                        .Select(x => x.GetString())
                        .Where(x => !string.IsNullOrWhiteSpace(x));

                    errorMessage = string.Join(", ", errors!);
                }

                return new CaptchaVerificationResult
                {
                    Success = false,
                    ErrorMessage = errorMessage ?? "Captcha verification failed."
                };
            }

            return new CaptchaVerificationResult
            {
                Success = true
            };
        }
    }
}