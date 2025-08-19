using System.Text;
using DreamInCodeApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────────────────
// DB: intenta leer "Default" o "DefaultConnection" (útil para Azure/AppSettings)
var connString =
    builder.Configuration.GetConnectionString("Default") ??
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connString))
    throw new InvalidOperationException("No se encontró la cadena de conexión 'Default' ni 'DefaultConnection'.");

builder.Services.AddDbContext<DreamInCodeContext>(opt =>
    opt.UseSqlServer(connString));

// CORS
builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowWeb", p => p
        .WithOrigins(
            "http://localhost:5173",
            "https://localhost:5173",
            "https://web-dream-in-code.vercel.app"
        )
        .SetIsOriginAllowed(origin =>
        {
            // ejemplo simple
            return origin.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase)
                   || origin.Equals("http://localhost:5173", StringComparison.OrdinalIgnoreCase)
                   || origin.Equals("https://localhost:5173", StringComparison.OrdinalIgnoreCase);
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});


var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Falta Jwt:Key en configuración.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// Controllers + Swagger (Bearer)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DreamInCodeApi",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega tu token (sin la palabra 'Bearer')."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DreamInCodeApi v1");
    c.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseCors("AllowWeb");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Healthcheck simple para Azure
app.MapGet("/healthz", () => Results.Ok("ok"));

app.Run();
