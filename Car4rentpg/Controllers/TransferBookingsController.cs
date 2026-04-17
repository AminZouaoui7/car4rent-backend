using Car4rentpg.DATA;
using Car4rentpg.DTOs;
using Car4rentpg.Models;
using Car4rentpg.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/transfer-bookings")]
    public class TransferBookingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public TransferBookingsController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTransferBookingDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PickupAirportId))
                return BadRequest(new { message = "L'aéroport de prise en charge est requis." });

            if (string.IsNullOrWhiteSpace(dto.DropoffCityId))
                return BadRequest(new { message = "La ville de destination est requise." });

            if (string.IsNullOrWhiteSpace(dto.HotelName))
                return BadRequest(new { message = "Le nom de l'hôtel est requis." });

            if (string.IsNullOrWhiteSpace(dto.FirstName))
                return BadRequest(new { message = "Le prénom est requis." });

            if (string.IsNullOrWhiteSpace(dto.LastName))
                return BadRequest(new { message = "Le nom est requis." });

            if (string.IsNullOrWhiteSpace(dto.Phone))
                return BadRequest(new { message = "Le téléphone est requis." });

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "L'email est requis." });

            if (dto.Passengers <= 0)
                return BadRequest(new { message = "Le nombre de passagers doit être supérieur à 0." });

            if (dto.LuggageCount < 0)
                return BadRequest(new { message = "Le nombre de bagages doit être positif." });

            var pickupAirport = await _context.Cities.FirstOrDefaultAsync(c => c.Id == dto.PickupAirportId);
            if (pickupAirport == null)
                return BadRequest(new { message = "Aéroport introuvable." });

            if (pickupAirport.Type.ToLower() != "airport")
                return BadRequest(new { message = "La prise en charge doit se faire uniquement depuis un aéroport." });

            var dropoffCity = await _context.Cities.FirstOrDefaultAsync(c => c.Id == dto.DropoffCityId);
            if (dropoffCity == null)
                return BadRequest(new { message = "Ville de destination introuvable." });

            if (dropoffCity.Type.ToLower() != "city")
                return BadRequest(new { message = "La destination doit être une ville." });

            var booking = new TransferBooking
            {
                PickupAirportId = dto.PickupAirportId,
                DropoffCityId = dto.DropoffCityId,
                HotelName = dto.HotelName,
                HotelAddress = dto.HotelAddress,
                TransferDate = DateTime.SpecifyKind(dto.TransferDate, DateTimeKind.Utc),
                LuggageCount = dto.LuggageCount,
                Passengers = dto.Passengers,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Phone = dto.Phone.Trim(),
                Email = dto.Email.Trim(),
                Status = TransferBookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.TransferBookings.Add(booking);
            await _context.SaveChangesAsync();

            await _emailService.SendTransferPendingEmailAsync(
                booking.Email,
                $"{booking.FirstName} {booking.LastName}",
                pickupAirport.Name,
                dropoffCity.Name,
                booking.HotelName,
                booking.HotelAddress,
                booking.TransferDate,
                booking.Passengers,
                booking.LuggageCount
            );

            return Ok(new
            {
                message = "Votre demande de transfert a bien été envoyée.",
                bookingId = booking.Id
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _context.TransferBookings
                .Include(x => x.PickupAirport)
                .Include(x => x.DropoffCity)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(bookings);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var booking = await _context.TransferBookings
                .Include(x => x.PickupAirport)
                .Include(x => x.DropoffCity)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (booking == null)
                return NotFound(new { message = "Demande introuvable." });

            return Ok(booking);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateTransferBookingStatusDto dto)
        {
            var booking = await _context.TransferBookings
                .Include(x => x.PickupAirport)
                .Include(x => x.DropoffCity)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (booking == null)
                return NotFound(new { message = "Demande introuvable." });

            if (!Enum.IsDefined(typeof(TransferBookingStatus), dto.Status))
                return BadRequest(new { message = "Statut invalide." });

            var previousStatus = booking.Status;
            booking.Status = (TransferBookingStatus)dto.Status;

            await _context.SaveChangesAsync();

            if (previousStatus != TransferBookingStatus.Confirmed &&
                booking.Status == TransferBookingStatus.Confirmed)
            {
                await _emailService.SendTransferConfirmedEmailAsync(
                    booking.Email,
                    $"{booking.FirstName} {booking.LastName}",
                    booking.PickupAirport?.Name ?? "Non renseigné",
                    booking.DropoffCity?.Name ?? "Non renseignée",
                    booking.HotelName,
                    booking.HotelAddress,
                    booking.TransferDate,
                    booking.Passengers,
                    booking.LuggageCount
                );
            }
            else if (previousStatus != TransferBookingStatus.Cancelled &&
                     booking.Status == TransferBookingStatus.Cancelled)
            {
                await _emailService.SendTransferCancelledEmailAsync(
                    booking.Email,
                    $"{booking.FirstName} {booking.LastName}",
                    booking.PickupAirport?.Name ?? "Non renseigné",
                    booking.DropoffCity?.Name ?? "Non renseignée",
                    booking.HotelName,
                    booking.HotelAddress,
                    booking.TransferDate,
                    booking.Passengers,
                    booking.LuggageCount
                );
            }

            return Ok(new { message = "Statut mis à jour avec succès." });
        }
    }
}