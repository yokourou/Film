using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WebApi;
using WebApi.Models;
using WebApi.Services;
using WebApi.Data; // Namespace for seeders
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using System.IO;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 📌 Register services
        builder.Services.AddScoped<RecommendationService>();
        builder.Services.AddScoped<PasswordHasher<User>>();
        builder.Services.AddSingleton<JwtService>();

        // 📌 Add database context
        builder.Services.AddDbContext<BddContext>(options =>
            options.UseSqlite("Data Source=Bdd.db"));

        // 📌 Add controllers and HttpClient
        builder.Services.AddControllers();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient<OmdbService>();

        // 📌 Configure authentication
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ClockSkew = TimeSpan.FromMinutes(10),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = "localhost:5000",
                    ValidIssuer = "localhost:5000",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("TheSecretKeyThatShouldBeStoredInTheConfiguration")
                    ),
                    RoleClaimType = ClaimTypes.Role
                };
            });

        builder.Services.AddAuthorization();

        // 📌 Configure Swagger for API documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(option =>
        {
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                              "Enter 'Bearer' followed by your JWT token in the text input below.\r\n\r\n" +
                              "Example: \"Bearer xxxxxxxxx\"",
            });

            option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                    new string[] { }
                }
            });
        });

        // 📌 Build the application
        var app = builder.Build();

        // 📌 Seed the database during application startup
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BddContext>();
            try
            {
                context.Database.Migrate(); // Apply migrations if any

                // Corrected: Define the correct CSV path
                var csvFilePath =  @".\data\Films.csv";
;

                // Seed data
                UserSeeder.SeedUsers(context); 
                FilmSeeder.SeedFilms(context, csvFilePath); // Correct parameter order
                FavouriteSeeder.SeedFavourites(context);

                Console.WriteLine("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ An error occurred during database seeding: {ex.Message}");
            }
        }

        // 📌 Development-only middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // 📌 Middleware for security and routing
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // 📌 Run the application
        app.Run();
    }
}
