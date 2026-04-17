using Car4rentpg.Models;
using Microsoft.EntityFrameworkCore;

namespace Car4rentpg.DATA
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // ===== ADMIN =====
            if (!context.AdminUsers.Any())
            {
                var admin = new AdminUser
                {
                    Email = "admin@car4rent.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Name = "Admin Principal"
                };

                var admin2 = new AdminUser
                {
                    Email = "admin2@car4rent.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Name = "Admin Test"
                };

                context.AdminUsers.AddRange(admin, admin2);
                await context.SaveChangesAsync();
            }

            // ===== CITIES =====
            if (!context.Cities.Any())
            {
                var cities = new List<City>
                {
                    new City { Id = Guid.NewGuid().ToString(), Name = "Tunis", Country = "Tunisie", Type = "city" },
                    new City { Id = Guid.NewGuid().ToString(), Name = "Sousse", Country = "Tunisie", Type = "city" },
                    new City { Id = Guid.NewGuid().ToString(), Name = "Monastir", Country = "Tunisie", Type = "city" },
                    new City { Id = Guid.NewGuid().ToString(), Name = "Aéroport Tunis Carthage", Country = "Tunisie", Type = "airport" }
                };

                context.Cities.AddRange(cities);
                await context.SaveChangesAsync();
            }

            // ===== VEHICLES =====
            if (!context.Vehicles.Any())
            {
                var category = new Category
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "SUV"
                };

                context.Categories.Add(category);

                var vehicles = new List<Vehicle>
                {
                    new Vehicle
                    {
                        Id = Guid.NewGuid().ToString(),
                        Brand = "BMW",
                        Model = "X1",
                        Slug = "bmw-x1",
                        BasePriceDay = 90,
                        Gearbox = "Auto",
                        Fuel = "Essence",
                        Seats = 5,
                        Bags = 3,
                        Available = true,
                        CategoryId = category.Id
                    },
                    new Vehicle
                    {
                        Id = Guid.NewGuid().ToString(),
                        Brand = "Mercedes",
                        Model = "Classe A",
                        Slug = "mercedes-a",
                        BasePriceDay = 80,
                        Gearbox = "Auto",
                        Fuel = "Diesel",
                        Seats = 5,
                        Bags = 2,
                        Available = true,
                        CategoryId = category.Id
                    }
                };

                context.Vehicles.AddRange(vehicles);
                await context.SaveChangesAsync();
            }
        }
    }
}