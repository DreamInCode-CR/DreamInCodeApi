using System.Globalization;
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

    // ------- DTOs -----
    public record UpsertReq(
        string  NombreMedicamento,
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
        string? Hora,
        string? HoraToma,
        bool Activo = true,
        bool RecordatorioHabilitado = false,
        short MinutosAntes = 0,
        string? MensajeRecordatorio = null
    );

    public record MedDto(
        int MedicamentoID,
        string NombreMedicamento,
        string? Dosis,
        string? Instrucciones,
        DateTime? FechaInicio,
        DateTime? FechaHasta,
        string? Hora,          
        string? HoraToma,      
        bool Lunes, bool Martes, bool Miercoles, bool Jueves,
        bool Viernes, bool Sabado, bool Domingo,
        bool Activo,
        bool RecordatorioHabilitado,
        short MinutosAntes,
        string? MensajeRecordatorio
    );

    private static string? ToHoraString(TimeSpan? t)
        => t.HasValue ? t.Value.ToString(@"hh\:mm", CultureInfo.InvariantCulture) : null;

    private static TimeSpan? ParseHora(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;

        // "HH:mm" ó "HH:mm:ss"
        if (TimeSpan.TryParse(s, CultureInfo.InvariantCulture, out var ts)) return ts;

        if (DateTime.TryParseExact(s.Trim(), "HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt.TimeOfDay;

        if (DateTime.TryParseExact(s.Trim(), "HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2))
            return dt2.TimeOfDay;

        return null; 
    }

    private static MedDto Map(Medicamento m) => new(
        m.MedicamentoID,
        m.NombreMedicamento,
        m.Dosis,
        m.Instrucciones,
        m.FechaInicio,
        m.FechaHasta,
        ToHoraString(m.HoraToma),   
        ToHoraString(m.HoraToma),
        m.Lunes, m.Martes, m.Miercoles, m.Jueves,
        m.Viernes, m.Sabado, m.Domingo,
        m.Activo,
        m.RecordatorioHabilitado,
        m.MinutosAntes,
        m.MensajeRecordatorio
    );

    // ------ Endpoints -------

    // GET /profiles/meds
    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var userId = JwtHelper.GetUserId(User);
        if (userId is null) return Unauthorized();

        var items = await _db.Medicamentos
            .Where(m => m.UsuarioID == userId.Value)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => Map(m))
            .ToListAsync();

        return Ok(new { items });
    }

    // POST /profiles/meds
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
            Dosis          = string.IsNullOrWhiteSpace(req.Dosis) ? null : req.Dosis!.Trim(),
            Instrucciones  = string.IsNullOrWhiteSpace(req.Instrucciones) ? null : req.Instrucciones!.Trim(),
            FechaInicio    = req.FechaInicio,
            FechaHasta     = req.FechaHasta,
            Lunes = req.Lunes, Martes = req.Martes, Miercoles = req.Miercoles,
            Jueves = req.Jueves, Viernes = req.Viernes, Sabado = req.Sabado, Domingo = req.Domingo,
            HoraToma = ParseHora(req.Hora ?? req.HoraToma), // <- acepta "hora" o "horaToma"
            Activo = req.Activo,
            CreatedAt = DateTime.UtcNow,
            RecordatorioHabilitado = req.RecordatorioHabilitado,
            MinutosAntes = req.MinutosAntes,
            MensajeRecordatorio = string.IsNullOrWhiteSpace(req.MensajeRecordatorio) ? null : req.MensajeRecordatorio!.Trim()
        };
        _db.Medicamentos.Add(e);
        await _db.SaveChangesAsync();

        return Ok(new { item = Map(e) });
    }

    // PUT /profiles/meds/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertReq req)
    {
        var userId = JwtHelper.GetUserId(User);
        if (userId is null) return Unauthorized();

        var e = await _db.Medicamentos
            .FirstOrDefaultAsync(x => x.MedicamentoID == id && x.UsuarioID == userId.Value);
        if (e == null) return NotFound();

        if (string.IsNullOrWhiteSpace(req.NombreMedicamento))
            return BadRequest(new { error = "Falta NombreMedicamento" });

        e.NombreMedicamento = req.NombreMedicamento.Trim();
        e.Dosis          = string.IsNullOrWhiteSpace(req.Dosis) ? null : req.Dosis!.Trim();
        e.Instrucciones  = string.IsNullOrWhiteSpace(req.Instrucciones) ? null : req.Instrucciones!.Trim();
        e.FechaInicio    = req.FechaInicio;
        e.FechaHasta     = req.FechaHasta;
        e.Lunes = req.Lunes; e.Martes = req.Martes; e.Miercoles = req.Miercoles; e.Jueves = req.Jueves;
        e.Viernes = req.Viernes; e.Sabado = req.Sabado; e.Domingo = req.Domingo;
        e.HoraToma = ParseHora(req.Hora ?? req.HoraToma);
        e.Activo   = req.Activo;
        e.RecordatorioHabilitado = req.RecordatorioHabilitado;
        e.MinutosAntes = req.MinutosAntes;
        e.MensajeRecordatorio = string.IsNullOrWhiteSpace(req.MensajeRecordatorio) ? null : req.MensajeRecordatorio!.Trim();
        
        await _db.SaveChangesAsync();
        return Ok(new { item = Map(e) });
    }

    // DELETE /profiles/meds/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = JwtHelper.GetUserId(User);
        if (userId is null) return Unauthorized();

        var e = await _db.Medicamentos
            .FirstOrDefaultAsync(x => x.MedicamentoID == id && x.UsuarioID == userId.Value);
        if (e == null) return NotFound();

        // Borrado lógico
        e.Activo = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
