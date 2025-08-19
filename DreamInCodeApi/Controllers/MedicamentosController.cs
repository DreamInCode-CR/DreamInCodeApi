using DreamInCodeApi.Data;
using DreamInCodeApi.Data.Models;
using DreamInCodeApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DreamInCodeApi.Controllers;

[ApiController]
[Route("profiles/meds")]
[Authorize]
public class MedicamentosController : ControllerBase
{
    private readonly DreamInCodeContext _db;
    public MedicamentosController(DreamInCodeContext db) => _db = db;

    public record UpsertReq(
        string NombreMedicamento,
        string? Dosis,
        string? Instrucciones,
        DateTime? FechaInicio,
        DateTime? FechaHasta,
        bool Lunes,
        bool Martes,
        bool Miercoles,
        bool Jueves,
        bool Viernes,
        bool Sabado,
        bool Domingo,
        TimeSpan? HoraToma,
        bool Activo = true
    );

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var userId = JwtHelper.GetUserId(User);
        if (userId is null) return Unauthorized();

        var meds = await _db.Medicamentos
            .Where(m => m.UsuarioID == userId.Value)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(new { items = meds });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertReq req)
    {
        var userId = JwtHelper.GetUserId(User);
        if (userId is null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(req.NombreMedicamento))
            return BadRequest(new { error = "Falta NombreMedicamento" });

        var e = new Medicamento
        {
            UsuarioID = userId.Value,
            NombreMedicamento = req.NombreMedicamento.Trim(),
            Dosis = string.IsNullOrWhiteSpace(req.Dosis) ? null : req.Dosis!.Trim(),
            Instrucciones = string.IsNullOrWhiteSpace(req.Instrucciones) ? null : req.Instrucciones!.Trim(),
            FechaInicio = req.FechaInicio,
            FechaHasta = req.FechaHasta,
            Lunes = req.Lunes,
            Martes = req.Martes,
            Miercoles = req.Miercoles,
            Jueves = req.Jueves,
            Viernes = req.Viernes,
            Sabado = req.Sabado,
            Domingo = req.Domingo,
            HoraToma = req.HoraToma,
            Activo = req.Activo,
            CreatedAt = DateTime.UtcNow
        };

        _db.Medicamentos.Add(e);
        await _db.SaveChangesAsync();

        return Ok(new { item = e });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertReq req)
    {
        var userId = JwtHelper.GetUserId(User);
        if (userId is null) return Unauthorized();

        var e = await _db.Medicamentos.FirstOrDefaultAsync(x => x.MedicamentoID == id && x.UsuarioID == userId.Value);
        if (e == null) return NotFound();

        if (string.IsNullOrWhiteSpace(req.NombreMedicamento))
            return BadRequest(new { error = "Falta NombreMedicamento" });

        e.NombreMedicamento = req.NombreMedicamento.Trim();
        e.Dosis = string.IsNullOrWhiteSpace(req.Dosis) ? null : req.Dosis!.Trim();
        e.Instrucciones = string.IsNullOrWhiteSpace(req.Instrucciones) ? null : req.Instrucciones!.Trim();
        e.FechaInicio = req.FechaInicio;
        e.FechaHasta = req.FechaHasta;
        e.Lunes = req.Lunes;
        e.Martes = req.Martes;
        e.Miercoles = req.Miercoles;
        e.Jueves = req.Jueves;
        e.Viernes = req.Viernes;
        e.Sabado = req.Sabado;
        e.Domingo = req.Domingo;
        e.HoraToma = req.HoraToma;
        e.Activo = req.Activo;

        await _db.SaveChangesAsync();
        return Ok(new { item = e });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = JwtHelper.GetUserId(User);
        if (userId is null) return Unauthorized();

        var e = await _db.Medicamentos.FirstOrDefaultAsync(x => x.MedicamentoID == id && x.UsuarioID == userId.Value);
        if (e == null) return NotFound();

        // borrado lógico
        e.Activo = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
