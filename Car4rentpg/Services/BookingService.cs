    using Car4rentpg.DATA;
    using Car4rentpg.DTOs;
    using Car4rentpg.Models;
    using Microsoft.EntityFrameworkCore;

    namespace Car4rentpg.Services
    {
        public class BookingService
        {
            private readonly AppDbContext _context;
            private readonly EmailService _emailService;
            private readonly PricingService _pricingService;
            private readonly CaptchaService _captchaService;

            // ===== TARIFS OPTIONS =====
            private const double SecondDriverPricePerDay = 1.0;
            private const double GpsPricePerDay = 2.0;
            private const double FullTankFlatPrice = 40.0;
            private const double BoosterSeatPricePerDay = 1.0;
            private const double BabySeatPricePerDay = 1.0;
            private const double ChildSeatPricePerDay = 1.0;
            private const double ProtectionPlusPricePerDay = 8.0;

            public BookingService(
                AppDbContext context,
                EmailService emailService,
                PricingService pricingService,
                CaptchaService captchaService)
            {
                _context = context;
                _emailService = emailService;
                _pricingService = pricingService;
                _captchaService = captchaService;
            }

        public async Task<Booking> CreateBookingAsync(CreateBookingDto dto)
        {
            if (dto == null)
                throw new Exception("Booking data is required.");

            await ValidateCaptchaAsync(dto.CaptchaToken);
            await CheckBookingAbuseAsync(dto);

            var startDate = dto.StartDate.Date;
            var endDate = dto.EndDate.Date;

            if (endDate <= startDate)
                throw new Exception("End date must be after start date.");

            var totalDays = (endDate - startDate).Days;

            if (totalDays < 2)
                throw new Exception("Minimum rental duration is 2 days.");

            var vehicle = await _context.Vehicles
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.Id == dto.VehicleId);

            if (vehicle == null)
                throw new Exception("Vehicle not found.");

            if (!vehicle.Available)
                throw new Exception("Vehicle is not available.");

            if (dto.HasSecondDriver)
            {
                if (string.IsNullOrWhiteSpace(dto.SecondDriverFirstName))
                    throw new Exception("Prénom deuxième conducteur obligatoire.");

                if (string.IsNullOrWhiteSpace(dto.SecondDriverLastName))
                    throw new Exception("Nom deuxième conducteur obligatoire.");

                if (string.IsNullOrWhiteSpace(dto.SecondDriverPhone))
                    throw new Exception("Téléphone deuxième conducteur obligatoire.");
            }

            if (dto.BoosterSeatQuantity < 0 || dto.BabySeatQuantity < 0 || dto.ChildSeatQuantity < 0)
                throw new Exception("Les quantités des options ne peuvent pas être négatives.");

            var pickupCity = await _context.Cities.FirstOrDefaultAsync(c => c.Id == dto.PickupCityId);
            if (pickupCity == null)
                throw new Exception("Pickup city not found.");

            City? returnCity = null;

            if (!string.IsNullOrWhiteSpace(dto.ReturnCityId))
            {
                returnCity = await _context.Cities.FirstOrDefaultAsync(c => c.Id == dto.ReturnCityId);

                if (returnCity == null)
                    throw new Exception("Return city not found.");
            }

            var pricing = await _pricingService.CalculateAsync(vehicle, startDate, endDate);
            var basePrice = Math.Round(pricing.TotalPrice, 2);

            var secondDriverAmount = dto.HasSecondDriver ? Math.Round(totalDays * SecondDriverPricePerDay, 2) : 0;
            var gpsAmount = dto.HasGps ? Math.Round(totalDays * GpsPricePerDay, 2) : 0;
            var fullTankAmount = dto.HasFullTank ? Math.Round(FullTankFlatPrice, 2) : 0;

            var boosterSeatAmount = dto.BoosterSeatQuantity > 0
                ? Math.Round(dto.BoosterSeatQuantity * totalDays * BoosterSeatPricePerDay, 2)
                : 0;

            var babySeatAmount = dto.BabySeatQuantity > 0
                ? Math.Round(dto.BabySeatQuantity * totalDays * BabySeatPricePerDay, 2)
                : 0;

            var childSeatAmount = dto.ChildSeatQuantity > 0
                ? Math.Round(dto.ChildSeatQuantity * totalDays * ChildSeatPricePerDay, 2)
                : 0;

            var protectionPlusAmount = dto.HasProtectionPlus
                ? Math.Round(totalDays * ProtectionPlusPricePerDay, 2)
                : 0;

            var originalPrice = Math.Round(
                basePrice +
                secondDriverAmount +
                gpsAmount +
                fullTankAmount +
                boosterSeatAmount +
                babySeatAmount +
                childSeatAmount +
                protectionPlusAmount,
                2
            );

            var discountAmount = 0.0;
            var totalPrice = originalPrice;
            string? promoCodeUsed = null;

            if (!string.IsNullOrWhiteSpace(dto.PromoCode))
            {
                var promo = await GetValidPromoCodeAsync(dto.PromoCode);

                discountAmount = Math.Round(originalPrice * (promo.DiscountPercentage / 100.0), 2);
                totalPrice = Math.Round(originalPrice - discountAmount, 2);

                if (totalPrice < 0)
                    totalPrice = 0;

                promoCodeUsed = promo.Code;
            }

            var depositAmount = Math.Round(totalPrice * 0.10, 2);
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc),

                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Phone = dto.Phone.Trim(),
                Email = normalizedEmail,
                Age = dto.Age,

                PickupCityId = dto.PickupCityId,
                ReturnCityId = string.IsNullOrWhiteSpace(dto.ReturnCityId) ? dto.PickupCityId : dto.ReturnCityId,

                VehicleId = dto.VehicleId,
                TotalDays = totalDays,

                HasSecondDriver = dto.HasSecondDriver,
                SecondDriverFirstName = dto.HasSecondDriver ? dto.SecondDriverFirstName?.Trim() : null,
                SecondDriverLastName = dto.HasSecondDriver ? dto.SecondDriverLastName?.Trim() : null,
                SecondDriverPhone = dto.HasSecondDriver ? dto.SecondDriverPhone?.Trim() : null,
                SecondDriverAmount = secondDriverAmount,

                HasGps = dto.HasGps,
                GpsAmount = gpsAmount,

                HasFullTank = dto.HasFullTank,
                FullTankAmount = fullTankAmount,

                BoosterSeatQuantity = dto.BoosterSeatQuantity,
                BoosterSeatAmount = boosterSeatAmount,

                BabySeatQuantity = dto.BabySeatQuantity,
                BabySeatAmount = babySeatAmount,

                ChildSeatQuantity = dto.ChildSeatQuantity,
                ChildSeatAmount = childSeatAmount,

                HasProtectionPlus = dto.HasProtectionPlus,
                ProtectionPlusAmount = protectionPlusAmount,

                OriginalPrice = originalPrice,
                DiscountAmount = discountAmount,
                TotalPrice = totalPrice,

                PricingSource = pricing.PricingSource,
                AppliedRule = pricing.AppliedRule,
                AppliedSeason = pricing.AppliedSeason,

                PromoCodeUsed = promoCodeUsed,

                DepositAmount = depositAmount,
                IsDepositPaid = false,
                DepositPaidAt = null,
                IsFullyPaid = false,
                FullyPaidAt = null,

                Status = BookingStatus.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var depositPayment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                BookingId = booking.Id,
                Type = "Deposit",
                Status = "Pending",
                Amount = depositAmount,
                Provider = "Fake",
                Currency = "EUR",
                Notes = "Acompte créé automatiquement à la création de la réservation.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            _context.Payments.Add(depositPayment);

            await _context.SaveChangesAsync();

            try
            {
                Console.WriteLine("📧 Début envoi email réservation...");

                await _emailService.SendBookingPendingEmailAsync(
                    booking.Email,
                    $"{booking.FirstName} {booking.LastName}",
                    $"{vehicle.Brand} {vehicle.Model}",
                    booking.StartDate,
                    booking.EndDate,
                    booking.TotalDays ?? 0,
                    booking.TotalPrice ?? 0,
                    pickupCity.Name,
                    returnCity?.Name ?? pickupCity.Name
                );

                Console.WriteLine("✅ Email réservation envoyé avec succès.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Email réservation non envoyé:");
                Console.WriteLine(ex.ToString());
            }

            return booking;
        }
        public async Task<BookingPricePreviewResponseDto> PreviewBookingPriceAsync(BookingPreviewDto dto)
            {
                if (dto == null)
                    throw new Exception("Preview data is required.");

                var startDate = dto.StartDate.Date;
                var endDate = dto.EndDate.Date;

                if (endDate <= startDate)
                    throw new Exception("End date must be after start date.");

                var totalDays = (endDate - startDate).Days;

                if (totalDays < 2)
                    throw new Exception("Minimum rental duration is 2 days.");

                var vehicle = await _context.Vehicles
                    .Include(v => v.Category)
                    .FirstOrDefaultAsync(v => v.Id == dto.VehicleId);

                if (vehicle == null)
                    throw new Exception("Vehicle not found.");

                if (!vehicle.Available)
                    throw new Exception("Vehicle is not available.");

                if (dto.HasSecondDriver)
                {
                    if (string.IsNullOrWhiteSpace(dto.SecondDriverFirstName))
                        throw new Exception("Prénom deuxième conducteur obligatoire.");

                    if (string.IsNullOrWhiteSpace(dto.SecondDriverLastName))
                        throw new Exception("Nom deuxième conducteur obligatoire.");

                    if (string.IsNullOrWhiteSpace(dto.SecondDriverPhone))
                        throw new Exception("Téléphone deuxième conducteur obligatoire.");
                }

                if (dto.BoosterSeatQuantity < 0 || dto.BabySeatQuantity < 0 || dto.ChildSeatQuantity < 0)
                    throw new Exception("Les quantités des options ne peuvent pas être négatives.");

                var pricing = await _pricingService.CalculateAsync(vehicle, startDate, endDate);
                double basePrice = Math.Round(pricing.TotalPrice, 2);

                double secondDriverAmount = dto.HasSecondDriver
                    ? Math.Round(totalDays * SecondDriverPricePerDay, 2)
                    : 0;

                double gpsAmount = dto.HasGps
                    ? Math.Round(totalDays * GpsPricePerDay, 2)
                    : 0;

                double fullTankAmount = dto.HasFullTank
                    ? Math.Round(FullTankFlatPrice, 2)
                    : 0;

                double boosterSeatAmount = dto.BoosterSeatQuantity > 0
                    ? Math.Round(dto.BoosterSeatQuantity * totalDays * BoosterSeatPricePerDay, 2)
                    : 0;

                double babySeatAmount = dto.BabySeatQuantity > 0
                    ? Math.Round(dto.BabySeatQuantity * totalDays * BabySeatPricePerDay, 2)
                    : 0;

                double childSeatAmount = dto.ChildSeatQuantity > 0
                    ? Math.Round(dto.ChildSeatQuantity * totalDays * ChildSeatPricePerDay, 2)
                    : 0;

                double protectionPlusAmount = dto.HasProtectionPlus
                    ? Math.Round(totalDays * ProtectionPlusPricePerDay, 2)
                    : 0;

                double originalPrice = Math.Round(
                    basePrice
                    + secondDriverAmount
                    + gpsAmount
                    + fullTankAmount
                    + boosterSeatAmount
                    + babySeatAmount
                    + childSeatAmount
                    + protectionPlusAmount,
                    2
                );

                double discountAmount = 0;
                double totalPrice = originalPrice;
                string? promoCodeUsed = null;
                bool promoApplied = false;

                if (!string.IsNullOrWhiteSpace(dto.PromoCode))
                {
                    var promo = await GetValidPromoCodeAsync(dto.PromoCode);

                    discountAmount = Math.Round(originalPrice * (promo.DiscountPercentage / 100.0), 2);
                    totalPrice = Math.Round(originalPrice - discountAmount, 2);

                    if (totalPrice < 0)
                        totalPrice = 0;

                    promoCodeUsed = promo.Code;
                    promoApplied = true;
                }

                return new BookingPricePreviewResponseDto
                {
                    OriginalPrice = originalPrice,
                    DiscountAmount = discountAmount,
                    TotalPrice = totalPrice,
                    PromoCodeUsed = promoCodeUsed,
                    PromoApplied = promoApplied,

                    SecondDriverAmount = secondDriverAmount,
                    GpsAmount = gpsAmount,
                    FullTankAmount = fullTankAmount,
                    BoosterSeatAmount = boosterSeatAmount,
                    BabySeatAmount = babySeatAmount,
                    ChildSeatAmount = childSeatAmount,
                    ProtectionPlusAmount = protectionPlusAmount
                };
            }

            public async Task<List<Booking>> GetAllBookingsAsync()

            {
                return await _context.Bookings
                    .Include(b => b.Vehicle)
                        .ThenInclude(v => v.Category)
                    .Include(b => b.PickupCity)
                    .Include(b => b.ReturnCity)
                    .Include(b => b.Payments)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();
            }

            public async Task<Booking?> UpdateStatusAsync(string id, BookingStatus status)
            {
                var booking = await _context.Bookings
                    .Include(b => b.Vehicle)
                        .ThenInclude(v => v.Category)
                    .Include(b => b.PickupCity)
                    .Include(b => b.ReturnCity)
                    .Include(b => b.Payments)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                    return null;

                var previousStatus = booking.Status;
                bool shouldSendConfirmedEmail = false;
                bool shouldSendCancelledEmail = false;

                if (previousStatus != BookingStatus.CONFIRMED && status == BookingStatus.CONFIRMED)
                {
                    if (!string.IsNullOrWhiteSpace(booking.PromoCodeUsed))
                    {
                        var promo = await GetValidPromoCodeAsync(booking.PromoCodeUsed);
                        promo.UsedCount += 1;
                    }

                    if (!booking.DepositAmount.HasValue || booking.DepositAmount <= 0)
                    {
                        booking.DepositAmount = Math.Round((booking.TotalPrice ?? 0) * 0.10, 2);
                    }

                    var depositPayment = booking.Payments
                        .FirstOrDefault(p => p.Type == "Deposit");

                    if (depositPayment == null)
                    {
                        depositPayment = new Payment
                        {
                            Id = Guid.NewGuid().ToString(),
                            BookingId = booking.Id,
                            Type = "Deposit",
                            Status = "Pending",
                            Amount = booking.DepositAmount ?? 0,
                            Provider = "Fake",
                            Currency = "EUR",
                            Notes = "Acompte créé automatiquement lors de la confirmation.",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.Payments.Add(depositPayment);
                    }

                    shouldSendConfirmedEmail = true;
                }

                if (previousStatus != BookingStatus.CANCELLED && status == BookingStatus.CANCELLED)
                {
                    if (previousStatus == BookingStatus.CONFIRMED && !string.IsNullOrWhiteSpace(booking.PromoCodeUsed))
                    {
                        var promo = await _context.PromoCodes
                            .FirstOrDefaultAsync(p => p.Code.ToUpper() == booking.PromoCodeUsed.ToUpper());

                        if (promo != null && promo.UsedCount > 0)
                        {
                            promo.UsedCount -= 1;
                        }
                    }

                    shouldSendCancelledEmail = true;
                }

                booking.Status = status;
                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                if (shouldSendConfirmedEmail)
                {
                    await _emailService.SendBookingConfirmedEmailAsync(
                        booking.Id,
                        booking.Email,
                        $"{booking.FirstName} {booking.LastName}",
                        $"{booking.Vehicle.Brand} {booking.Vehicle.Model}",
                        booking.StartDate,
                        booking.EndDate,
                        booking.TotalDays,
                        booking.TotalPrice,
                        booking.PickupCity.Name,
                        booking.ReturnCity?.Name ?? booking.PickupCity.Name
                    );
                }

                if (shouldSendCancelledEmail)
                {
                    await _emailService.SendBookingCancelledEmailAsync(
                        booking.Email,
                        $"{booking.FirstName} {booking.LastName}",
                        $"{booking.Vehicle.Brand} {booking.Vehicle.Model}",
                        booking.StartDate,
                        booking.EndDate,
                        booking.TotalDays,
                        booking.TotalPrice,
                        booking.PickupCity.Name,
                        booking.ReturnCity?.Name ?? booking.PickupCity.Name
                    );
                }

                return booking;
            }

            public async Task<Booking?> MarkDepositPaidAsync(string id)
            {
                var booking = await _context.Bookings
                    .Include(b => b.Vehicle)
                        .ThenInclude(v => v.Category)
                    .Include(b => b.PickupCity)
                    .Include(b => b.ReturnCity)
                    .Include(b => b.Payments)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                    return null;

                if (booking.Status != BookingStatus.CONFIRMED)
                    throw new Exception("Deposit can only be paid after booking confirmation.");

                if (!booking.DepositAmount.HasValue || booking.DepositAmount <= 0)
                {
                    booking.DepositAmount = Math.Round((booking.TotalPrice ?? 0) * 0.10, 2);
                }

                var depositPayment = booking.Payments
                    .Where(p => p.Type == "Deposit")
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefault();

                if (depositPayment == null)
                {
                    depositPayment = new Payment
                    {
                        Id = Guid.NewGuid().ToString(),
                        BookingId = booking.Id,
                        Type = "Deposit",
                        Status = "Pending",
                        Amount = booking.DepositAmount ?? 0,
                        Provider = "Fake",
                        Currency = "EUR",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Payments.Add(depositPayment);
                }

                if (booking.IsDepositPaid && depositPayment.Status == "Paid")
                    return booking;

                booking.IsDepositPaid = true;
                booking.DepositPaidAt = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;

                depositPayment.Status = "Paid";
                depositPayment.PaidAt = DateTime.UtcNow;
                depositPayment.UpdatedAt = DateTime.UtcNow;
                depositPayment.TransactionId ??= $"FAKE-DEP-{Guid.NewGuid():N}";
                depositPayment.Provider = "Fake";
                depositPayment.Notes = "Paiement acompte simulé depuis la page fake payment.";

                await _context.SaveChangesAsync();

                await _emailService.SendDepositPaidEmailAsync(
                    booking.Email,
                    $"{booking.FirstName} {booking.LastName}",
                    booking.DepositAmount ?? 0
                );

                return booking;
            }

            public async Task<Booking?> MarkFullyPaidAsync(string id)
            {
                var booking = await _context.Bookings
                    .Include(b => b.Vehicle)
                        .ThenInclude(v => v.Category)
                    .Include(b => b.PickupCity)
                    .Include(b => b.ReturnCity)
                    .Include(b => b.Payments)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                    return null;

                if (booking.Status != BookingStatus.CONFIRMED)
                    throw new Exception("Booking must be confirmed before marking it as fully paid.");

                if (!booking.IsDepositPaid)
                    throw new Exception("Deposit must be paid before marking the booking as fully paid.");

                if (booking.IsFullyPaid)
                    return booking;

                var totalPrice = booking.TotalPrice ?? 0;
                var depositAmount = booking.DepositAmount ?? 0;
                var remainingAmount = Math.Round(totalPrice - depositAmount, 2);

                if (remainingAmount < 0)
                    remainingAmount = 0;

                var existingBalancePayment = booking.Payments
                    .Where(p => p.Type == "Balance" && p.Status == "Paid")
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefault();

                if (existingBalancePayment == null && remainingAmount > 0)
                {
                    var balancePayment = new Payment
                    {
                        Id = Guid.NewGuid().ToString(),
                        BookingId = booking.Id,
                        Type = "Balance",
                        Status = "Paid",
                        Amount = remainingAmount,
                        Provider = "Admin",
                        Currency = "EUR",
                        TransactionId = $"BAL-{Guid.NewGuid():N}",
                        Notes = "Solde final marqué comme payé depuis l’admin.",
                        CreatedAt = DateTime.UtcNow,
                        PaidAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Payments.Add(balancePayment);
                }

                booking.IsFullyPaid = true;
                booking.FullyPaidAt = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _emailService.SendFullyPaidEmailAsync(
                    booking.Email,
                    $"{booking.FirstName} {booking.LastName}",
                    booking.TotalPrice ?? 0
                );

                return await _context.Bookings
                    .Include(b => b.Vehicle)
                        .ThenInclude(v => v.Category)
                    .Include(b => b.PickupCity)
                    .Include(b => b.ReturnCity)
                    .Include(b => b.Payments)
                    .FirstOrDefaultAsync(b => b.Id == id);
            }

            public async Task<Booking?> GetBookingByIdAsync(string id)
            {
                return await _context.Bookings
                    .Include(b => b.Vehicle)
                        .ThenInclude(v => v.Category)
                    .Include(b => b.PickupCity)
                    .Include(b => b.ReturnCity)
                    .Include(b => b.Payments)
                    .FirstOrDefaultAsync(b => b.Id == id);
            }

        private async Task ValidateCaptchaAsync(string? captchaToken)
        {
            var captchaResult = await _captchaService.VerifyAsync(captchaToken);

            if (!captchaResult.Success)
                throw new Exception($"Captcha invalide. Veuillez réessayer. Détail: {captchaResult.ErrorMessage}");
        }

        private async Task CheckBookingAbuseAsync(CreateBookingDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var now = DateTime.UtcNow;
            var tenMinutesAgo = now.AddMinutes(-10);

            var bookingsQuery = _context.Bookings
                .AsNoTracking()
                .Where(b => b.Email == normalizedEmail);

            var recentReservationsByEmail = await bookingsQuery
                .CountAsync(b => b.CreatedAt >= tenMinutesAgo);

            if (recentReservationsByEmail >= 3)
                throw new Exception("Trop de réservations ont été créées avec cet email en peu de temps. Veuillez réessayer plus tard.");

            var pendingReservationsByEmail = await bookingsQuery
                .CountAsync(b => b.Status == BookingStatus.PENDING);

            if (pendingReservationsByEmail >= 2)
                throw new Exception("Vous avez déjà 2 réservations en attente avec cet email.");

            var startUtc = DateTime.SpecifyKind(dto.StartDate.Date, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(dto.EndDate.Date, DateTimeKind.Utc);

            var exactDuplicateExists = await bookingsQuery
                .AnyAsync(b =>
                    b.VehicleId == dto.VehicleId &&
                    b.StartDate == startUtc &&
                    b.EndDate == endUtc &&
                    b.CreatedAt >= tenMinutesAgo);

            if (exactDuplicateExists)
                throw new Exception("Une réservation identique a déjà été créée récemment avec cet email.");
        }

        private async Task<PromoCode> GetValidPromoCodeAsync(string rawPromoCode)
            {
                var normalizedCode = rawPromoCode.Trim().ToUpper();

                var promo = await _context.PromoCodes
                    .FirstOrDefaultAsync(p => p.Code.ToUpper() == normalizedCode);

                if (promo == null)
                    throw new Exception("Invalid promo code.");

                if (!promo.IsActive)
                    throw new Exception("This promo code is disabled.");

                var now = DateTime.UtcNow;

                if (promo.StartDate.HasValue)
                {
                    var startInclusive = promo.StartDate.Value.Date;

                    if (now.Date < startInclusive)
                        throw new Exception("This promo code is not active yet.");
                }

                if (promo.EndDate.HasValue)
                {
                    var endInclusive = promo.EndDate.Value.Date.AddDays(1).AddTicks(-1);

                    if (now > endInclusive)
                        throw new Exception("This promo code has expired.");
                }

                if (promo.MaxUses.HasValue && promo.UsedCount >= promo.MaxUses.Value)
                    throw new Exception("This promo code has reached its maximum number of uses.");

                if (promo.DiscountPercentage <= 0 || promo.DiscountPercentage > 100)
                    throw new Exception("Invalid promo code percentage.");

                return promo;
            }
        }
    }