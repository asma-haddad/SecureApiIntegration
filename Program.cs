using ExpenseAuthApi.Data;
using ExpenseAuthApi.Middleware;
using ExpenseAuthApi.Model;
using ExpenseAuthApi.Services.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing");

// ==========================
// Controllers + OpenAPI
// ==========================

builder.Services.AddControllers();
builder.Services.AddOpenApi();


// ==========================
// Database
// ==========================

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration
            .GetConnectionString("DefaultConnection"));
});


// ==========================
// Services
// ==========================

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<AccessTokenService>();

builder.Services.AddScoped<RefreshTokenService>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();


// ==========================
// Middleware
// ==========================

builder.Services.AddTransient<HandleExceptionMiddleware>();


// ==========================
// Authentication
// ==========================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();


// ==========================
// Build App
// ==========================

var app = builder.Build();


// ==========================
// Middleware Pipeline
// ==========================

app.UseMiddleware<HandleExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();