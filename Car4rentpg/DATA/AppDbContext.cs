using Car4rentpg.Models;
using Microsoft.EntityFrameworkCore;

namespace Car4rentpg.DATA
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Season> Seasons => Set<Season>();
        public DbSet<TariffSettings> TariffSettings => Set<TariffSettings>();
        public DbSet<PricingRule> PricingRules => Set<PricingRule>();
        public DbSet<BlackoutPeriod> BlackoutPeriods => Set<BlackoutPeriod>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
        public DbSet<LongTermRentalRequest> LongTermRentalRequests => Set<LongTermRentalRequest>();
        public DbSet<TransferBooking> TransferBookings => Set<TransferBooking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // UNIQUE INDEXES
            // =========================
            modelBuilder.Entity<AdminUser>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.Token)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.Slug)
                .IsUnique();

            modelBuilder.Entity<PromoCode>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelBuilder.Entity<TariffSettings>()
                .HasIndex(t => new { t.VehicleId, t.Type })
                .IsUnique();

            // =========================
            // DEFAULT VALUES
            // =========================
            modelBuilder.Entity<City>()
                .Property(c => c.Type)
                .HasDefaultValue("city");

            modelBuilder.Entity<AdminUser>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<AdminUser>()
                .Property(x => x.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<AdminUser>()
                .Property(x => x.FailedLoginAttempts)
                .HasDefaultValue(0);

            modelBuilder.Entity<RefreshToken>()
                .Property(x => x.CreatedAtUtc)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<RefreshToken>()
                .Property(x => x.IsRevoked)
                .HasDefaultValue(false);

            modelBuilder.Entity<Category>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<City>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Season>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<TariffSettings>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<TariffSettings>()
                .Property(x => x.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<PricingRule>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<BlackoutPeriod>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Booking>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Booking>()
                .Property(x => x.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Payment>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<PromoCode>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<PromoCode>()
                .Property(x => x.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.Status)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.IsQuoteSent)
                .HasDefaultValue(false);

            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.Status)
                .HasDefaultValue(TransferBookingStatus.Pending);

            // =========================
            // RELATIONS
            // =========================
            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.AdminUser)
                .WithMany(a => a.RefreshTokens)
                .HasForeignKey(r => r.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Category)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Vehicle)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.PickupCity)
                .WithMany()
                .HasForeignKey(b => b.PickupCityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.ReturnCity)
                .WithMany()
                .HasForeignKey(b => b.ReturnCityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BlackoutPeriod>()
                .HasOne(b => b.Vehicle)
                .WithMany(v => v.Blackouts)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TariffSettings>()
                .HasOne(t => t.Vehicle)
                .WithMany(v => v.TariffSettings)
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PricingRule>()
                .HasOne(p => p.Vehicle)
                .WithMany(v => v.PricingRules)
                .HasForeignKey(p => p.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PricingRule>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LongTermRentalRequest>()
                .HasOne(x => x.PickupCity)
                .WithMany()
                .HasForeignKey(x => x.PickupCityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LongTermRentalRequest>()
                .HasOne(x => x.Vehicle)
                .WithMany()
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TransferBooking>()
                .HasOne(x => x.PickupAirport)
                .WithMany()
                .HasForeignKey(x => x.PickupAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferBooking>()
                .HasOne(x => x.DropoffCity)
                .WithMany()
                .HasForeignKey(x => x.DropoffCityId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // INDEXES
            // =========================
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.AdminUserId);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.ExpiresAtUtc);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.VehicleId, b.StartDate, b.EndDate });

            modelBuilder.Entity<Booking>()
    .HasIndex(b => new { b.Email, b.CreatedAt });

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.Email, b.Status });

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.Email, b.VehicleId, b.StartDate, b.EndDate, b.CreatedAt });

            modelBuilder.Entity<PricingRule>()
                .HasIndex(p => p.VehicleId);

            modelBuilder.Entity<PricingRule>()
                .HasIndex(p => p.CategoryId);

            modelBuilder.Entity<PricingRule>()
                .HasIndex(p => new { p.StartDate, p.EndDate });

            modelBuilder.Entity<PricingRule>()
                .HasIndex(p => p.IsActive);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.BookingId);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.Status);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.TransactionId);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.SessionId);

            modelBuilder.Entity<LongTermRentalRequest>()
                .HasIndex(x => x.PickupCityId);

            modelBuilder.Entity<LongTermRentalRequest>()
                .HasIndex(x => x.VehicleId);

            modelBuilder.Entity<LongTermRentalRequest>()
                .HasIndex(x => x.Status);

            modelBuilder.Entity<LongTermRentalRequest>()
                .HasIndex(x => x.CreatedAt);

            modelBuilder.Entity<TransferBooking>()
                .HasIndex(x => x.PickupAirportId);

            modelBuilder.Entity<TransferBooking>()
                .HasIndex(x => x.DropoffCityId);

            modelBuilder.Entity<TransferBooking>()
                .HasIndex(x => x.Status);

            modelBuilder.Entity<TransferBooking>()
                .HasIndex(x => x.TransferDate);

            modelBuilder.Entity<TransferBooking>()
                .HasIndex(x => x.CreatedAt);

            // =========================
            // ADMIN USER CONFIG
            // =========================
            modelBuilder.Entity<AdminUser>()
                .Property(a => a.Email)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<AdminUser>()
                .Property(a => a.Password)
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<AdminUser>()
                .Property(a => a.Name)
                .HasMaxLength(150)
                .IsRequired(false);

            modelBuilder.Entity<AdminUser>()
                .Property(a => a.LockoutEndUtc)
                .IsRequired(false);

            modelBuilder.Entity<AdminUser>()
                .Property(a => a.LastLoginAtUtc)
                .IsRequired(false);

            modelBuilder.Entity<AdminUser>()
                .Property(a => a.LastLoginIp)
                .HasMaxLength(100)
                .IsRequired(false);

            // =========================
            // REFRESH TOKEN CONFIG
            // =========================
            modelBuilder.Entity<RefreshToken>()
                .Property(r => r.Token)
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<RefreshToken>()
                .Property(r => r.AdminUserId)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<RefreshToken>()
                .Property(r => r.ReplacedByToken)
                .HasMaxLength(500)
                .IsRequired(false);

            modelBuilder.Entity<RefreshToken>()
                .Property(r => r.RevokedAtUtc)
                .IsRequired(false);

            // =========================
            // BOOKING CONFIG
            // =========================
            modelBuilder.Entity<Booking>()
                .Property(x => x.TotalPrice)
                .HasColumnType("double precision");

            modelBuilder.Entity<Booking>()
                .Property(x => x.OriginalPrice)
                .HasColumnType("double precision");

            modelBuilder.Entity<Booking>()
                .Property(x => x.DiscountAmount)
                .HasColumnType("double precision");

            modelBuilder.Entity<Booking>()
                .Property(x => x.SecondDriverAmount)
                .HasColumnType("double precision")
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.GpsAmount)
                .HasColumnType("double precision")
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.FullTankAmount)
                .HasColumnType("double precision")
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.BoosterSeatAmount)
                .HasColumnType("double precision")
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.BabySeatAmount)
                .HasColumnType("double precision")
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.ChildSeatAmount)
                .HasColumnType("double precision")
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.ProtectionPlusAmount)
                .HasColumnType("double precision")
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.DepositAmount)
                .HasColumnType("double precision")
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.HasSecondDriver)
                .HasDefaultValue(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.HasGps)
                .HasDefaultValue(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.HasFullTank)
                .HasDefaultValue(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.HasProtectionPlus)
                .HasDefaultValue(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.BoosterSeatQuantity)
                .HasDefaultValue(0);

            modelBuilder.Entity<Booking>()
                .Property(x => x.BabySeatQuantity)
                .HasDefaultValue(0);

            modelBuilder.Entity<Booking>()
                .Property(x => x.ChildSeatQuantity)
                .HasDefaultValue(0);

            modelBuilder.Entity<Booking>()
                .Property(x => x.SecondDriverFirstName)
                .HasMaxLength(100)
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.SecondDriverLastName)
                .HasMaxLength(100)
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.SecondDriverPhone)
                .HasMaxLength(30)
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.PromoCodeUsed)
                .HasMaxLength(100)
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.PricingSource)
                .HasMaxLength(50)
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.AppliedRule)
                .HasMaxLength(150)
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.AppliedSeason)
                .HasMaxLength(200)
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.ReturnCityId)
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.DepositPaidAt)
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.FullyPaidAt)
                .IsRequired(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.IsDepositPaid)
                .HasDefaultValue(false);

            modelBuilder.Entity<Booking>()
                .Property(x => x.IsFullyPaid)
                .HasDefaultValue(false);

            // =========================
            // VEHICLE CONFIG
            // =========================
            modelBuilder.Entity<Vehicle>()
                .Property(x => x.Brand)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Vehicle>()
                .Property(x => x.Model)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Vehicle>()
                .Property(x => x.Slug)
                .HasMaxLength(180)
                .IsRequired();

            modelBuilder.Entity<Vehicle>()
                .Property(x => x.Gearbox)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Vehicle>()
                .Property(x => x.Fuel)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Vehicle>()
                .Property(x => x.Image)
                .HasMaxLength(1000)
                .IsRequired(false);

            modelBuilder.Entity<Vehicle>()
                .Property(x => x.BasePriceDay)
                .HasColumnType("double precision");

            modelBuilder.Entity<Vehicle>()
                .Property(x => x.Available)
                .HasDefaultValue(true);

            // =========================
            // TARIFF SETTINGS CONFIG
            // =========================
            modelBuilder.Entity<TariffSettings>()
                .Property(x => x.Type)
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<TariffSettings>()
                .Property(x => x.PriceStart)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<TariffSettings>()
                .Property(x => x.Price3Days)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<TariffSettings>()
                .Property(x => x.Price4To6Days)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<TariffSettings>()
                .Property(x => x.Price7To15Days)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<TariffSettings>()
                .Property(x => x.Price16To29Days)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<TariffSettings>()
                .Property(x => x.Price1Month)
                .HasColumnType("numeric(18,2)");

            // =========================
            // PRICING RULE CONFIG
            // =========================
            modelBuilder.Entity<PricingRule>()
                .Property(x => x.VehicleId)
                .IsRequired(false);

            modelBuilder.Entity<PricingRule>()
                .Property(x => x.CategoryId)
                .IsRequired(false);

            modelBuilder.Entity<PricingRule>()
                .Property(x => x.PricePerDay)
                .HasColumnType("double precision");

            modelBuilder.Entity<PricingRule>()
                .Property(x => x.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<PricingRule>()
                .Property(x => x.Label)
                .HasMaxLength(150)
                .IsRequired(false);

            // =========================
            // PAYMENT CONFIG
            // =========================
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("double precision");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Type)
                .HasMaxLength(30)
                .HasDefaultValue("Deposit");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Provider)
                .HasMaxLength(30);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Currency)
                .HasMaxLength(10);

            modelBuilder.Entity<Payment>()
                .Property(p => p.SessionId)
                .HasMaxLength(255);

            modelBuilder.Entity<Payment>()
                .Property(p => p.TransactionId)
                .HasMaxLength(255);

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentUrl)
                .HasMaxLength(1000)
                .IsRequired(false);

            modelBuilder.Entity<Payment>()
                .Property(p => p.UpdatedAt)
                .IsRequired(false);

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaidAt)
                .IsRequired(false);

            // =========================
            // PROMO CODE CONFIG
            // =========================
            modelBuilder.Entity<PromoCode>()
                .Property(p => p.Code)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<PromoCode>()
                .Property(p => p.DiscountPercentage)
                .HasColumnType("double precision");

            modelBuilder.Entity<PromoCode>()
                .Property(p => p.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<PromoCode>()
                .Property(p => p.StartDate)
                .IsRequired(false);

            modelBuilder.Entity<PromoCode>()
                .Property(p => p.EndDate)
                .IsRequired(false);

            modelBuilder.Entity<PromoCode>()
                .Property(p => p.MaxUses)
                .IsRequired(false);

            modelBuilder.Entity<PromoCode>()
                .Property(p => p.UsedCount)
                .HasDefaultValue(0);

            // =========================
            // LONG TERM RENTAL REQUEST CONFIG
            // =========================
            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.Phone)
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.Email)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.PickupCityId)
                .IsRequired();

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.VehicleId)
                .IsRequired(false);

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.DurationMonths)
                .IsRequired();

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.Notes)
                .HasMaxLength(1000)
                .IsRequired(false);

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.ProposedMonthlyPrice)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            modelBuilder.Entity<LongTermRentalRequest>()
                .Property(x => x.ProposedTotalPrice)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            // =========================
            // TRANSFER BOOKING CONFIG
            // =========================
            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.PickupAirportId)
                .IsRequired();

            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.DropoffCityId)
                .IsRequired();

            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.HotelName)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.HotelAddress)
                .HasMaxLength(250)
                .IsRequired(false);

            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.LuggageCount)
                .IsRequired();

            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.Passengers)
                .IsRequired();

            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.Phone)
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.Email)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<TransferBooking>()
                .Property(x => x.Status)
                .IsRequired();
        }
    }
}