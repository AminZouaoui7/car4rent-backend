using Car4rentpg.DTOs;
using Car4rentpg.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public BookingsController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // ===============================
        // CREATE BOOKING
        // ===============================
        [EnableRateLimiting("public-booking")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Booking data is required." });

                dto.StartDate = DateTime.SpecifyKind(dto.StartDate.Date, DateTimeKind.Utc);
                dto.EndDate = DateTime.SpecifyKind(dto.EndDate.Date, DateTimeKind.Utc);

                var result = await _bookingService.CreateBookingAsync(dto);

                return Ok(new
                {
                    id = result.Id,
                    message = "Réservation créée avec succès.",
                    totalPrice = result.TotalPrice,
                    originalPrice = result.OriginalPrice,
                    discountAmount = result.DiscountAmount,
                    promoCodeUsed = result.PromoCodeUsed,
                    depositAmount = result.DepositAmount,
                    status = result.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    detail = ex.InnerException?.Message
                });
            }
        }
        // ===============================
        // PREVIEW PRICE
        // ===============================
        [HttpPost("preview-price")]
        public async Task<IActionResult> PreviewPrice([FromBody] BookingPreviewDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Preview data is required." });

                dto.StartDate = DateTime.SpecifyKind(dto.StartDate.Date, DateTimeKind.Utc);
                dto.EndDate = DateTime.SpecifyKind(dto.EndDate.Date, DateTimeKind.Utc);

                var result = await _bookingService.PreviewBookingPriceAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // ===============================
        // ADMIN - GET ALL BOOKINGS
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // ===============================
        // ADMIN - UPDATE STATUS
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateBookingStatusDto dto)
        {
            try
            {
                var booking = await _bookingService.UpdateStatusAsync(id, dto.Status);

                if (booking == null)
                    return NotFound(new { message = "Booking not found." });

                return Ok(new
                {
                    message = "Booking status updated successfully.",
                    booking
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // ===============================
        // MARK DEPOSIT PAID
        // ===============================
        [HttpPut("{id}/mark-deposit-paid")]
        public async Task<IActionResult> MarkDepositPaid(string id)
        {
            try
            {
                var booking = await _bookingService.MarkDepositPaidAsync(id);

                if (booking == null)
                    return NotFound(new { message = "Booking not found." });

                return Ok(new
                {
                    message = "Deposit marked as paid successfully.",
                    booking
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // ===============================
        // ADMIN - GET BOOKING BY ID
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);

                if (booking == null)
                    return NotFound(new { message = "Booking not found." });

                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // ===============================
        // ADMIN - MARK FULLY PAID
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/mark-fully-paid")]
        public async Task<IActionResult> MarkFullyPaid(string id)
        {
            try
            {
                var booking = await _bookingService.MarkFullyPaidAsync(id);

                if (booking == null)
                    return NotFound(new { message = "Booking not found." });

                return Ok(new
                {
                    message = "Booking marked as fully paid successfully.",
                    booking
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}