using System.Text;
using DreamInCodeApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔎 (Opcional en local) mostrar PII en logs de auth
IdentityModelEventSource.ShowPII = true;

// DB
builder.Services.AddDbContext<DreamInCodeContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// CORS (ajusta orígenes si usas otro puerto para el Front)
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p =>
        p.WithOrigins("http://localhost:5173", "https://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

// JWT (CLAVE EN TEXTO PLANO, NO base64)
var jwt = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,          
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };

        o.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx => { 
                Console.WriteLine($"AuthFailed: {ctx.Exception.GetType().Name}: {ctx.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = ctx => { 
                Console.WriteLine($"Challenge: {ctx.Error} - {ctx.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Swagger con esquema HTTP Bearer (no ApiKey)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DreamInCodeApi", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega solo el token (Swagger antepone 'Bearer ')."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme
            { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
          Array.Empty<string>() }
    });
});

// (Opcional) imprimir config cargada, útil para detectar appsettings pisados
Console.WriteLine($"JWT -> Issuer={jwt["Issuer"]}, Audience={jwt["Audience"]}, KeyLen={jwt["Key"]?.Length}");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();   
app.UseAuthorization();

app.MapControllers();

app.Run();
