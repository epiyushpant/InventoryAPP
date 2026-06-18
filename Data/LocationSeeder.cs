using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Inventory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Data
{
    public static class LocationSeeder
    {
        public static async Task SeedLocationsAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Seed Countries
            if (!await context.Countries.AnyAsync())
            {
                var countries = new[]
                {
                    new Country { CountryName = "Nepal", IsoCode = "NP" },
                    new Country { CountryName = "India", IsoCode = "IN" },
                    new Country { CountryName = "China", IsoCode = "CN" },
                    new Country { CountryName = "United States", IsoCode = "US" },
                    new Country { CountryName = "United Kingdom", IsoCode = "GB" },
                    new Country { CountryName = "Australia", IsoCode = "AU" },
                    new Country { CountryName = "Japan", IsoCode = "JP" }
                };

                await context.Countries.AddRangeAsync(countries);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Seeded default countries.");
            }

            var nepal = await context.Countries.FirstOrDefaultAsync(c => c.CountryName == "Nepal");
            if (nepal == null) return;

            // 2. Seed Nepal Divisions
            if (!await context.Provinces.AnyAsync())
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "nepal-divisions.json");
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(AppContext.BaseDirectory, "nepal-divisions.json");
                }

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("⚠️ nepal-divisions.json file not found. Skipping Nepal divisions seed.");
                    return;
                }

                try
                {
                    var jsonString = await File.ReadAllTextAsync(filePath);
                    using var doc = JsonDocument.Parse(jsonString);
                    var root = doc.RootElement;

                    foreach (var provinceProperty in root.EnumerateObject())
                    {
                        var provinceName = provinceProperty.Name.Trim();
                        var province = new Province
                        {
                            ProvinceName = provinceName,
                            CountryID = nepal.CountryID
                        };
                        await context.Provinces.AddAsync(province);
                        await context.SaveChangesAsync(); // Save to get ProvinceID

                        foreach (var districtProperty in provinceProperty.Value.EnumerateObject())
                        {
                            var districtName = districtProperty.Name.Trim();
                            var district = new District
                            {
                                DistrictName = districtName,
                                ProvinceID = province.ProvinceID
                            };
                            await context.Districts.AddAsync(district);
                            await context.SaveChangesAsync(); // Save to get DistrictID

                            foreach (var municipalityProperty in districtProperty.Value.EnumerateObject())
                            {
                                var municipalityName = municipalityProperty.Name.Trim();
                                var type = "Municipality";
                                if (municipalityName.Contains("Rural Municipality", StringComparison.OrdinalIgnoreCase))
                                {
                                    type = "Rural Municipality";
                                }
                                else if (municipalityName.Contains("Sub-Metropolitan", StringComparison.OrdinalIgnoreCase) || 
                                         municipalityName.Contains("Sub Metropolitan", StringComparison.OrdinalIgnoreCase))
                                {
                                    type = "Sub-Metropolitan City";
                                }
                                else if (municipalityName.Contains("Metropolitan", StringComparison.OrdinalIgnoreCase))
                                {
                                    type = "Metropolitan City";
                                }

                                var municipality = new Municipality
                                {
                                    MunicipalityName = municipalityName,
                                    Type = type,
                                    DistrictID = district.DistrictID
                                };
                                await context.Municipalities.AddAsync(municipality);
                            }
                        }
                    }

                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Seeded Nepal Provinces, Districts, and Municipalities successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error seeding Nepal divisions: {ex.Message}");
                }
            }
        }
    }
}
