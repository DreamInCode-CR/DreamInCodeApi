using BCrypt.Net;
using DreamInCodeApi.Data;
using DreamInCodeApi.Data.Dtos;
using DreamInCodeApi.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DreamInCodeApi.Data.Models;

namespace DreamInCodeApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly DreamInCodeContext _db;
    private readonly IConfiguration _cfg;

    public AuthController(DreamInCodeContext db, IConfiguration cfg)
    {
        _db = db; _cfg = cfg;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Correo) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "correo y password son requeridos" });

        var exists = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == req.Correo);
        if (exists != null) return Conflict(new { error = "El correo ya está registrado" });

        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 10);

        DateOnly? fn = req.FechaNacimiento;

        var user = new Usuarios
        {
            Correo            = req.Correo,
            PasswordHash      = hash,
            PasswordUpdatedAt = DateTime.UtcNow,
            Nombre            = req.Nombre,
            PrimerApellido    = req.PrimerApellido,
            SegundoApellido   = req.SegundoApellido,
            FechaNacimiento   = fn,             
            Telefono          = req.Telefono,
            Direccion         = req.Direccion
        };

        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();

        var token = JwtHelper.CreateToken(user.UsuarioID, _cfg);
        return new AuthResponse(
            user.UsuarioID, user.Correo, user.Nombre, user.PrimerApellido, user.SegundoApellido, token
        );
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Correo) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "correo y password son requeridos" });

        var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == req.Correo);
        if (user == null) return Unauthorized(new { error = "Credenciales inválidas" });

        if (string.IsNullOrEmpty(user.PasswordHash))
            return Unauthorized(new { error = "Cuenta sin contraseña configurada. Contacte al admin." });

        var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!ok) return Unauthorized(new { error = "Credenciales inválidas" });

        var token = JwtHelper.CreateToken(user.UsuarioID, _cfg);
        return new AuthResponse(
            user.UsuarioID, user.Correo, user.Nombre, user.PrimerApellido, user.SegundoApellido, token
        );
    }
}
