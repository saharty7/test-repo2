using System.Text.Json.Serialization;
using HotelBookingPlatform.Common;
using HotelBookingPlatform.Data;
using HotelBookingPlatform.Services;
using HotelBookingPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hotel Booking Platform API",
        Version = "v1",
        Description = "API for managing rooms, bookings, guests and reviews for a boutique hotel."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "opaque-token",
        In = ParameterLocation.Header,
        Description = "Staff login token. Log in via /api/auth/login, then enter: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? "Host=localhost;Port=5432;Database=hotelbooking;Username=hotelbooking;Password=hotelbooking";

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddSingleton<StaffTokenStore>();

builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services
    .AddAuthentication(AuthSchemes.StaffBearer)
    .AddScheme<AuthenticationSchemeOptions, StaffAuthenticationHandler>(AuthSchemes.StaffBearer, _ => { });

builder.Services.AddAuthorization();

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
const int maxDatabaseConnectionAttempts = 10;
var retryDelay = TimeSpan.FromSeconds(3);

for (var attempt = 1; attempt <= maxDatabaseConnectionAttempts; attempt++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        DataSeeder.Seed(db);
        startupLogger.LogInformation("Connected to the database on attempt {Attempt}.", attempt);
        break;
    }
    catch (Exception ex) when (attempt < maxDatabaseConnectionAttempts)
    {
        startupLogger.LogWarning(
            ex,
            "Database not ready yet (attempt {Attempt}/{MaxAttempts}). Retrying in {DelaySeconds}s...",
            attempt,
            maxDatabaseConnectionAttempts,
            retryDelay.TotalSeconds);
        Thread.Sleep(retryDelay);
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Booking Platform API v1");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
