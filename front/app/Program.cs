using app.Components;
using app.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Blazored.LocalStorage;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace app;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Validate JWT settings
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        if (jwtSettings == null)
        {
            throw new ArgumentNullException("Jwt settings are missing in configuration.");
        }

        string issuer = jwtSettings["Issuer"] ?? throw new ArgumentNullException("Issuer cannot be null.");
        string audience = jwtSettings["Audience"] ?? throw new ArgumentNullException("Audience cannot be null.");
        string key = jwtSettings["Key"] ?? throw new ArgumentNullException("Key cannot be null.");

        // Add services to the container
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Configure HttpClient for API communication
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5102/") });

        // Register application services
        builder.Services.AddScoped<CustomStateAuthProvider>();
        builder.Services.AddScoped<FilmService>();
        builder.Services.AddScoped<UserServiceFront>();
        builder.Services.AddScoped<FavouriteService>();
        builder.Services.AddScoped<ProtectedLocalStorage>(); // Secure storage
        builder.Services.AddScoped<RecommendationServicefront>();
        builder.Services.AddScoped<OmdbService>(); // Ensure OmdbService is registered

        // Blazored Local Storage
        builder.Services.AddBlazoredLocalStorage();

        // Configure Authentication and Authorization
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.Zero // Optional: Remove delay in token expiration
            };
        });

        builder.Services.AddScoped<AuthenticationStateProvider, CustomStateAuthProvider>();
        builder.Services.AddAuthorizationCore();

        // Add Anti-Forgery services
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts(); // Add HSTS for production
        }

        // Static files and HTTPS redirection
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        // Routing middleware
        app.UseRouting();

        // Authentication and Authorization Middleware
        app.UseAuthentication();
        app.UseAuthorization();

        // Antiforgery Middleware (must be after UseAuthentication and UseAuthorization)
        app.UseAntiforgery();

        // Map Razor components
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
        });

        app.Run();
    }
}
