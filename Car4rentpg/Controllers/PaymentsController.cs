using System.Text.Json;
using Car4rentpg.DATA;
using Car4rentpg.Models;
using Car4rentpg.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly KonnectService _konnectService;
        private readonly KonnectSettings _settings;

        public PaymentsController(
            AppDbContext context,
            KonnectService konnectService,
            IOptions<KonnectSettings> options)
        {
            _context = context;
            _konnectService = konnectService;
            _settings = options.Value;
        }

        // =========================
        // CREATE DEPOSIT PAYMENT
        // =========================
        [HttpPost("create-deposit-session/{bookingId}")]
        public async Task<IActionResult> CreateDepositSession(string bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return NotFound(new { message = "Réservation introuvable." });

            if (booking.Status != BookingStatus.CONFIRMED)
                return BadRequest(new { message = "Réservation non confirmée." });

            if (booking.IsDepositPaid)
                return BadRequest(new { message = "Acompte déjà payé." });

            var totalPrice = booking.TotalPrice ?? 0;

            if (totalPrice <= 0)
                return BadRequest(new { message = "Montant invalide." });

            // 💡 Config dynamique
            var depositPercent = _settings.DepositPercent;
            var currency = _settings.Currency;

            var depositAmount = Math.Round(totalPrice * depositPercent / 100, 2);

            booking.DepositAmount = depositAmount;

            var existing = booking.Payments
                .FirstOrDefault(p => p.Type == "Deposit" && p.Status == "Pending");

            var orderId = $"BOOKING-{booking.Id}-{DateTime.UtcNow.Ticks}";

            try
            {
                var result = await _konnectService.CreatePaymentAsync(
                    orderId,
                    depositAmount,
                    booking.FirstName,
                    booking.LastName,
                    booking.Email,
                    booking.Phone,
                    $"Acompte {depositPercent}% réservation {booking.Vehicle.Brand} {booking.Vehicle.Model}"
                );

                if (existing == null)
                {
                    existing = new Payment
                    {
                        BookingId = booking.Id,
                        Type = "Deposit",
                        Status = "Pending",
                        Amount = depositAmount,
                        Provider = "Konnect",
                        SessionId = result.paymentRef,
                        Currency = currency,
                        PaymentUrl = result.payUrl,
                        Notes = "Konnect Payment créé",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Payments.Add(existing);
                }
                else
                {
                    existing.SessionId = result.paymentRef;
                    existing.PaymentUrl = result.payUrl;
                    existing.UpdatedAt = DateTime.UtcNow;
                }

                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    url = result.payUrl,
                    amount = depositAmount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Erreur Konnect",
                    error = ex.Message
                });
            }
        }

        // =========================
        // WEBHOOK KONNECT
        // =========================
        [HttpGet("konnect/webhook")]
        public async Task<IActionResult> KonnectWebhook([FromQuery] string payment_ref)
        {
            if (string.IsNullOrWhiteSpace(payment_ref))
                return BadRequest("payment_ref manquant");

            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.SessionId == payment_ref);

            if (payment == null)
                return NotFound("Paiement introuvable");

            // ⚠️ sécurité : éviter double traitement
            if (payment.Status == "Paid")
                return Ok("Déjà traité");

            try
            {
                using var details = await _konnectService.GetPaymentDetailsAsync(payment_ref);

                var status = details.RootElement
                    .GetProperty("payment")
                    .GetProperty("status")
                    .GetString();

                var transactionId = details.RootElement
                    .GetProperty("payment")
                    .GetProperty("id")
                    .GetString();

                if (status == "completed")
                {
                    payment.Status = "Paid";
                    payment.TransactionId = transactionId;
                    payment.PaidAt = DateTime.UtcNow;
                    payment.UpdatedAt = DateTime.UtcNow;

                    payment.Booking.IsDepositPaid = true;
                    payment.Booking.DepositPaidAt = DateTime.UtcNow;
                    payment.Booking.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    payment.Status = "Failed";
                    payment.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok("Webhook traité");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // =========================
        // STATUS
        // =========================
        [HttpGet("status/{bookingId}")]
        public async Task<IActionResult> GetPaymentStatus(string bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return NotFound();

            var payment = booking.Payments
                .Where(p => p.Type == "Deposit")
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault();

            return Ok(new
            {
                bookingId,
                isDepositPaid = booking.IsDepositPaid,
                payment
            });
        }
    }
}