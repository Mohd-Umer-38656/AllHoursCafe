using AllHoursCafe.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AllHoursCafe.API
{
    public class MigrateAndSeed
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting database migration and seeding...");

            try
            {
                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                // Get connection string
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                Console.WriteLine($"Using connection string: {connectionString}");

                // Create service collection
                var services = new ServiceCollection();

                // Configure logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                // Configure DbContext
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 32));
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseMySql(connectionString, serverVersion));

                // Register DbSeeder
                services.AddScoped<DbSeeder>();
                services.AddScoped<UpdateImageUrls>();

                // Build service provider
                var serviceProvider = services.BuildServiceProvider();

                // Get DbContext
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<MigrateAndSeed>>();

                    // Apply migrations
                    Console.WriteLine("Applying migrations...");
                    await dbContext.Database.MigrateAsync();
                    Console.WriteLine("Migrations applied successfully.");

                    // Seed database
                    Console.WriteLine("Seeding database...");
                    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
                    await seeder.SeedAsync();
                    Console.WriteLine("Database seeded successfully.");

                    // Update image URLs
                    Console.WriteLine("Updating image URLs...");
                    var imageUrlUpdater = scope.ServiceProvider.GetRequiredService<UpdateImageUrls>();
                    await imageUrlUpdater.UpdateMenuItemImageUrlsAsync();
                    Console.WriteLine("Image URLs updated successfully.");
                }

                Console.WriteLine("Database migration and seeding completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine(ex.InnerException.StackTrace);
                }
            }
        }
    }
}
