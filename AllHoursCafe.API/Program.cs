using Microsoft.EntityFrameworkCore;
using AllHoursCafe.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Pomelo.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Handle circular references in JSON serialization
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64; // Increase max depth if needed
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use property names as-is
        options.JsonSerializerOptions.WriteIndented = true; // Make JSON more readable
    });

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 32));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MultiAuth";
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "AllHoursCafe.Auth";
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"] ?? "your-secret-key")),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
})
.AddPolicyScheme("MultiAuth", "MultiAuth", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        // Check if JWT token is present in the request
        string authorization = context.Request.Headers["Authorization"];
        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
            return JwtBearerDefaults.AuthenticationScheme;

        // Otherwise use cookies
        return CookieAuthenticationDefaults.AuthenticationScheme;
    };
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AllHoursCafe API",
        Version = "v1",
        Description = "API for AllHoursCafe web application",
        Contact = new OpenApiContact
        {
            Name = "AllHoursCafe Support",
            Email = "support@allhourscafe.com"
        }
    });

    // Configure JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
});

// Register DbSeeder and UpdateImageUrls
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddScoped<UpdateImageUrls>();

var app = builder.Build();

// Ensure database is created, migrations are applied, and data is seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // Apply migrations instead of just ensuring the database is created
        context.Database.Migrate();

        // Seed the database
        var seeder = services.GetRequiredService<DbSeeder>();
        seeder.SeedAsync().Wait(); // Use Wait() instead of await since we're not in an async context

        // Update image URLs
        var imageUrlUpdater = services.GetRequiredService<UpdateImageUrls>();
        imageUrlUpdater.UpdateMenuItemImageUrlsAsync().Wait();

        // Log success
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database migrations applied, data seeded, and image URLs updated successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying database migrations or seeding data.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// Enable CORS
app.UseCors();

// Serve static files
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// Use session middleware
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Configure routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
