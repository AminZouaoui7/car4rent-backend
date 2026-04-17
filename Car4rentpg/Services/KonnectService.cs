using System.Text;
using System.Text.Json;
using Car4rentpg.Models;
using Microsoft.Extensions.Options;

namespace Car4rentpg.Services
{
    public class KonnectService
    {
        private readonly HttpClient _httpClient;
        private readonly KonnectSettings _settings;

        public KonnectService(HttpClient httpClient, IOptions<KonnectSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<(string paymentRef, string payUrl)> CreatePaymentAsync(
            string orderId,
            double amount,
            string firstName,
            string lastName,
            string email,
            string phone,
            string description)
        {
            var amountInSmallestUnit = ConvertToSmallestUnit(amount, _settings.Currency);

            var payload = new
            {
                receiverWalletId = _settings.ReceiverWalletId,
                token = _settings.Currency,
                amount = amountInSmallestUnit,
                type = "immediate",
                description = description,
                acceptedPaymentMethods = new[] { "bank_card" },
                lifespan = 60,
                checkoutForm = true,
                addPaymentFeesToAmount = false,
                firstName = firstName,
                lastName = lastName,
                email = email,
                phoneNumber = phone,
                orderId = orderId,
                webhook = _settings.WebhookUrl,
                successUrl = _settings.SuccessUrl,
                failUrl = _settings.CancelUrl,
                theme = "light"
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.BaseUrl}/payments/init-payment"
            );

            request.Headers.Add("x-api-key", _settings.ApiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erreur Konnect init-payment: {content}");

            using var doc = JsonDocument.Parse(content);

            var paymentRef = doc.RootElement.GetProperty("paymentRef").GetString();
            var payUrl = doc.RootElement.GetProperty("payUrl").GetString();

            if (string.IsNullOrWhiteSpace(paymentRef) || string.IsNullOrWhiteSpace(payUrl))
                throw new Exception("Réponse Konnect invalide.");

            return (paymentRef, payUrl);
        }

        public async Task<JsonDocument> GetPaymentDetailsAsync(string paymentRef)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.BaseUrl}/payments/{paymentRef}"
            );

            request.Headers.Add("x-api-key", _settings.ApiKey);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erreur Konnect get payment: {content}");

            return JsonDocument.Parse(content);
        }

        private static long ConvertToSmallestUnit(double amount, string currency)
        {
            if (currency.Equals("TND", StringComparison.OrdinalIgnoreCase))
                return (long)Math.Round(amount * 1000, MidpointRounding.AwayFromZero);

            return (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
        }
    }
}