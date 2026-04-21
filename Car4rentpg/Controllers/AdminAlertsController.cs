using Car4rentpg.DATA;
using Car4rentpg.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/admin/alerts")]
    [Authorize(Roles = "Admin")]
    public class AdminAlertsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminAlertsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var bookingsCount = await _context.Bookings
                .CountAsync(x => x.Status == BookingStatus.PENDING);

            var longTermCount = await _context.LongTermRentalRequests
                .CountAsync(x => x.Status == "Pending");

            var transfersCount = await _context.TransferBookings
                .CountAsync(x => x.Status == TransferBookingStatus.Pending);

            return Ok(new
            {
                bookingsCount,
                longTermCount,
                transfersCount,
                reservationsTotal = bookingsCount + longTermCount,
                globalTotal = bookingsCount + longTermCount + transfersCount
            });
        }
    }
}