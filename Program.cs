using Inventory.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Inventory.Data;
using Inventory.Services;
using Inventory.Middleware;
using System.Text;

namespace Inventory
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // ============================================
            // SERVICES CONFIGURATION (correct order)
            // ============================================

            // 1. Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Identity - register before configuring authentication
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // 3. Authentication (JWT) - now configure JWT
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtKey = builder.Configuration["Jwt:Key"];
                var jwtIssuer = builder.Configuration["Jwt:Issuer"];
                var jwtAudience = builder.Configuration["Jwt:Audience"];

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? "temporary-secret-key-replace-in-production")),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        if (!string.IsNullOrEmpty(authHeader))
                        {
                            Console.WriteLine($"✅ [JWT] Authorization header received");
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var userName = context.Principal?.Identity?.Name;
                        Console.WriteLine($"✅ [JWT] Token validated successfully for user: {userName}");
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"❌ [JWT] Authentication failed");
                        Console.WriteLine($"   Exception Type: {context.Exception?.GetType().Name}");
                        Console.WriteLine($"   Message: {context.Exception?.Message}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"🔐 [JWT] Challenge issued - returning 401 Unauthorized");
                        return Task.CompletedTask;
                    }
                };
            });

            // 4. Authorization
            builder.Services.AddAuthorization();

            // 5. CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // 6. Repositories
            builder.Services.AddScoped<CategoryRepository>();
            builder.Services.AddScoped<SupplierRepository>();
            builder.Services.AddScoped<CustomerRepository>();
            builder.Services.AddScoped<LocationRepository>();
            builder.Services.AddScoped<ProductRepository>();
            builder.Services.AddScoped<InventoryRepository>();
            builder.Services.AddScoped<PurchaseOrderRepository>();
            builder.Services.AddScoped<PurchaseOrderDetailRepository>();
            builder.Services.AddScoped<SaleRepository>();
            builder.Services.AddScoped<SaleDetailRepository>();
            builder.Services.AddScoped<StockMovementRepository>();
            builder.Services.AddScoped<PostRepository>();
            builder.Services.AddScoped<PurchaseRequisitionRepository>();
            builder.Services.AddScoped<GRNRepository>();
            builder.Services.AddScoped<DeliveryNoteRepository>();
            builder.Services.AddScoped<StockAdjustmentRepository>();
            builder.Services.AddScoped<StockTransferRepository>();
            builder.Services.AddScoped<SalesInvoiceRepository>();
            builder.Services.AddScoped<ReportRepository>();
            builder.Services.AddScoped<UnitConversionRepository>();
            builder.Services.AddScoped<TenantCapabilityService>();
            builder.Services.AddScoped<RolePermissionService>();
            builder.Services.AddScoped<ITenantContext, TenantContext>();

            // 7. Controllers
            builder.Services.AddControllers();

            // 8. Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Inventory API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGci...\""
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            if (string.IsNullOrEmpty(builder.Configuration["Jwt:Key"])) throw new Exception("JWT Key not found in configuration.");

            var app = builder.Build();

            // Seed initial roles and admin user
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    Task.Run(async () => await DataSeeder.SeedRolesAndAdminAsync(services)).Wait();
                    Console.WriteLine("✅ Database seeded with Roles and Admin user.");
                    
                    Task.Run(async () => await LocationSeeder.SeedLocationsAsync(services)).Wait();

                    var caps = services.GetRequiredService<TenantCapabilityService>();
                    Task.Run(async () => await caps.GetOrCreateDefaultTenantAsync()).Wait();
                    Console.WriteLine("✅ Default tenant + capabilities seeded.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error seeding data: {ex.Message}");
                }
            }

            // ============================================
            // MIDDLEWARE PIPELINE - ORDER IS CRITICAL!
            // ============================================

            // 1. Swagger (Development only)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                Console.WriteLine("✅ Swagger enabled at /swagger");
            }

            // 2. HTTPS Redirect (disabled in development for frontend HTTP calls)
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
                Console.WriteLine("✅ HTTPS redirection enabled");
            }
            else
            {
                Console.WriteLine("⚠️ HTTPS redirection disabled in development");
            }

            // 3. CORS - MUST be BEFORE Authentication
            app.UseCors("AllowAll");
            Console.WriteLine("✅ CORS enabled for all origins");

            // 4. Authentication - MUST be BEFORE Authorization
            app.UseAuthentication();
            Console.WriteLine("✅ Authentication middleware enabled");

            app.UseMiddleware<TenantResolutionMiddleware>();
            Console.WriteLine("✅ Tenant resolution middleware enabled");

            // 5. Authorization - MUST be AFTER Authentication
            app.UseAuthorization();
            Console.WriteLine("✅ Authorization middleware enabled");

            // 6. Map Controllers
            app.MapControllers();
            Console.WriteLine("✅ Controllers mapped");

            Console.WriteLine("\n🚀 Inventory API is starting...");
            if (app.Environment.IsDevelopment())
            {
                Console.WriteLine("📝 Documentation: http://localhost:5201/swagger");
            }
            else
            {
                Console.WriteLine("📝 Documentation: https://localhost:7010/swagger");
            }
            Console.WriteLine("🔐 JWT Authentication is enabled\n");

            app.Run();
        }
    }
}