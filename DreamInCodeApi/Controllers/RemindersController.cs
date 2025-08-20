using System.Globalization;
using DreamInCodeApi.Data;
using DreamInCodeApi.Data.Models;
using DreamInCodeApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DreamInCodeApi.Controllers;

[ApiController]
[Route("reminders")]
[Authorize]
public class RemindersController : ControllerBase
{
    private readonly DreamInCodeContext _db;
    private readonly ILogger<RemindersController> _logger;
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(60);
    private const string TimeZoneId = "America/Costa_Rica";

    public RemindersController(DreamInCodeContext db, ILogger<RemindersController> logger)
    {
        _db = db; _logger = logger;
    }

    public sealed class ReminderDto
    {
        public int MedicamentoID { get; set; }
        public string Nombre { get; set; } = default!;
        public string Mensaje { get; set; } = default!;
        public string HoraProgramada { get; set; } = default!; // "HH:mm"
    }

    [HttpGet("now")]
    public async Task<ActionResult<IEnumerable<ReminderDto>>> GetNow(CancellationToken ct)
    {
        var userId = JwtHelper.GetUserId(User);
        if (userId is null) return Unauthorized();

        var tz = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var today = nowLocal.Date;
        var dow = nowLocal.DayOfWeek;

        bool IsDayEnabled(Medicamento m) => dow switch
        {
            DayOfWeek.Monday    => m.Lunes,
            DayOfWeek.Tuesday   => m.Martes,
            DayOfWeek.Wednesday => m.Miercoles,
            DayOfWeek.Thursday  => m.Jueves,
            DayOfWeek.Friday    => m.Viernes,
            DayOfWeek.Saturday  => m.Sabado,
            DayOfWeek.Sunday    => m.Domingo,
            _ => false
        };

        var meds = await _db.Medicamentos
            .Where(m => m.UsuarioID == userId.Value && m.Activo && m.RecordatorioHabilitado && m.HoraToma != null)
            .ToListAsync(ct);

        var due = new List<ReminderDto>();

        foreach (var m in meds)
        {
            if (!IsDayEnabled(m)) continue;

            var hora = m.HoraToma!.Value; // TimeSpan
            var trigger = today.Add(hora).AddMinutes(-m.MinutosAntes);

            if (trigger <= nowLocal && (nowLocal - trigger) <= Window)
            {
                var already = await _db.ReminderLog.AnyAsync(x =>
                    x.UsuarioID == userId.Value &&
                    x.MedicamentoID == m.MedicamentoID &&
                    x.FechaProgramada == today &&
                    x.HoraProgramada == hora, ct);

                if (already) continue;

                _db.ReminderLog.Add(new ReminderLog
                {
                    UsuarioID = userId.Value,
                    MedicamentoID = m.MedicamentoID,
                    FechaProgramada = today,
                    HoraProgramada = hora,
                    DisparadoAt = DateTime.UtcNow
                });

                due.Add(new ReminderDto
                {
                    MedicamentoID = m.MedicamentoID,
                    Nombre = m.NombreMedicamento,
                    Mensaje = string.IsNullOrWhiteSpace(m.MensajeRecordatorio)
                        ? $"Esto es un recordatorio para tomarte tu medicamento {m.NombreMedicamento}"
                        : m.MensajeRecordatorio!,
                    HoraProgramada = new DateTime(today.Year, today.Month, today.Day, hora.Hours, hora.Minutes, 0)
                        .ToString("HH:mm", CultureInfo.InvariantCulture)
                });
            }
        }

        if (due.Count > 0) await _db.SaveChangesAsync(ct);
        return Ok(due);
    }
}
