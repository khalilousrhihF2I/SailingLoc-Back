using Api.Middleware;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Notifications;
using Core.Interfaces.PasswordReset;
using Core.Interfaces.Services.Notifications;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Messaging;
using Infrastructure.Services;
using Infrastructure.Services.Messaging;
using Infrastructure.Templates;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SendGrid;
using Serilog;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();
builder.Host.UseSerilog();

// Db
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")));


// Identity
builder.Services.AddIdentity<AppUser, AppRole>(opt =>
{
    opt.Password.RequiredLength = 8;
    opt.Password.RequireNonAlphanumeric = false;
}).AddEntityFrameworkStores<ApplicationDbContext>()
  .AddDefaultTokenProviders();

// JWT
var signingKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY")
    ?? builder.Configuration["Jwt:SigningKey"] 
    ?? throw new Exception("JWT_SIGNING_KEY is missing");

var keyBytes = Encoding.UTF8.GetBytes(signingKey);
var key = new SymmetricSecurityKey(keyBytes);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(signingKey)),

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

// CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Default", p => p
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// CSRF / Anti-forgery protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.HttpOnly = false; // accessible by JS to read and send back in header
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Repository + Service
builder.Services.AddScoped<BoatRepository>();
builder.Services.AddScoped<IBoatService, BoatService>();
// Register destinations repository and service
builder.Services.AddScoped<DestinationRepository>();
builder.Services.AddScoped<IDestinationService, DestinationService>();
// Register availability repository and service
builder.Services.AddScoped<AvailabilityRepository>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
// Register booking repository and service
builder.Services.AddScoped<BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
// Register review repository and service
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
// Register user repo and service
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
// Register dashboard service
builder.Services.AddScoped<IDashboardService, DashboardService>();
// Register renter dashboard service
builder.Services.AddScoped<IRenterDashboardService, RenterDashboardService>();
// Register owner dashboard service
builder.Services.AddScoped<IOwnerDashboardService, OwnerDashboardService>();
// Register home service
builder.Services.AddScoped<IHomeService, HomeService>();
// Audit logging
builder.Services.AddScoped<IAuditService, AuditService>();
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Startup API", Version = "v1" });

    // JWT scheme
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };

    c.AddSecurityDefinition("Bearer", jwtScheme); 
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
            Array.Empty<string>()
        }
    });
});


builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

// SendGrid client
builder.Services.AddSingleton<ISendGridClient>(_ =>
    new SendGridClient(builder.Configuration["SendGrid:dataFlow"]));


// Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

// Azure Blob Storage — GED / document storage
var blobConnectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING")
    ?? builder.Configuration["AzureBlobStorage:ConnectionString"]
    ?? "";
if (!string.IsNullOrWhiteSpace(blobConnectionString))
{
    builder.Services.AddSingleton(new Azure.Storage.Blobs.BlobServiceClient(blobConnectionString));
    builder.Services.AddScoped<IFileStorageService, Infrastructure.Services.AzureBlobStorageService>();
}
else
{
    builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
}

builder.Services.AddTransient<ErrorHandlingMiddleware>();

// Messaging
builder.Services.AddScoped<IEmailSender, SendGridEmailSender>();
builder.Services.AddScoped<ISmsSender, TwilioSmsSender>(); // stub si pas encore branch�

// Templates
builder.Services.AddScoped<ITemplateRenderer, SimpleTemplateRenderer>();

// Repos
builder.Services.AddScoped<IPasswordResetCodeRepository, PasswordResetCodeRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

// Configure controllers JSON options to handle object cycles
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignore cycles when serializing to avoid exceptions from EF navigation properties
        options.JsonSerializerOptions.ReferenceHandler =
           System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        // Keep a reasonable max depth
        options.JsonSerializerOptions.MaxDepth = 64;
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Apply migrations & seed
await DbSeeder.SeedAsync(app.Services, builder.Configuration);

app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseStaticFiles();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

// CSRF token endpoint — frontend calls this to get a token
app.MapGet("/api/v1/csrf-token", (Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery, HttpContext context) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    return Results.Ok(new { token = tokens.RequestToken });
}).AllowAnonymous();

app.MapControllers();
app.MapHealthChecks("/healthz");
 

app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

