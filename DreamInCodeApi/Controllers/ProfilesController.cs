using DreamInCodeApi.Data.Dtos;
using DreamInCodeApi.Data.Models;
using DreamInCodeApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DreamInCodeApi.Controllers;

[ApiController]
[Route("profiles")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly DreamInCodeApi.Data.DreamInCodeContext _db;
    public ProfilesController(DreamInCodeApi.Data.DreamInCodeContext db) => _db = db;

    // GET combinado para poblar el formulario de Setup
    [HttpGet("setup")]
    public async Task<IActionResult> GetSetup()
    {
        var userId = JwtHelper.GetUserId(User);
        if (userId is null) return Unauthorized(new { error = "token inválido" });

        var u = await _db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UsuarioID == userId.Value);
        if (u is null) return NotFound(new { error = "usuario no encontrado" });

        var p = await _db.Perfiles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UsuarioID == userId.Value);

        return Ok(new {
            // Usuario
            nombre = u.Nombre,
            apellido1 = u.PrimerApellido,
            apellido2 = u.SegundoApellido,
            fechaNacimiento = u.FechaNacimiento,
            correo = u.Correo,
            telefono = u.Telefono,
            direccion = u.Direccion,
            // Perfil
            preferencias = p?.Preferencias,
            notasMedicas = p?.NotasMedicas
        });
    }

    // PUT/POST para guardar Setup (Usuario + Perfil)
    [HttpPut("setup")]
    public async Task<IActionResult> UpsertSetup([FromBody] SetupRequest req)
    {
        var userId = JwtHelper.GetUserId(User);
        if (userId is null) return Unauthorized(new { error = "token inválido" });

        var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.UsuarioID == userId.Value);
        if (u is null) return NotFound(new { error = "usuario no encontrado" });

        // Actualiza Usuario (solo si vienen valores)
        if (req.Nombre        != null) u.Nombre         = req.Nombre;
        if (req.Apellido1     != null) u.PrimerApellido = req.Apellido1;
        if (req.Apellido2     != null) u.SegundoApellido= req.Apellido2;
        if (req.FechaNacimiento.HasValue) u.FechaNacimiento = req.FechaNacimiento;
        if (req.Telefono      != null) u.Telefono       = req.Telefono;
        if (req.Direccion     != null) u.Direccion      = req.Direccion;

        // Upsert de Perfil (Preferencias / NotasMedicas)
        var p = await _db.Perfiles.FirstOrDefaultAsync(x => x.UsuarioID == userId.Value);
        if (p is null)
        {
            p = new Perfiles { UsuarioID = userId.Value };
            _db.Perfiles.Add(p);
        }

        if (req.Preferencias != null) p.Preferencias = req.Preferencias;
        if (req.NotasMedicas != null) p.NotasMedicas = req.NotasMedicas;

        await _db.SaveChangesAsync();

        return Ok(new { ok = true });
    }
}
