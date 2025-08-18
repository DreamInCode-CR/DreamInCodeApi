using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("debug")]
public class DebugController : ControllerBase
{
    private readonly IConfiguration _cfg;
    public DebugController(IConfiguration cfg) => _cfg = cfg;

    [HttpGet("token")]
    [AllowAnonymous]
    public IActionResult InspectToken()
    {
        var auth = Request.Headers.Authorization.ToString(); // "Bearer eyJ..."
        if (string.IsNullOrWhiteSpace(auth)) return BadRequest(new { error = "No Authorization header" });

        var parts = auth.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Authorization header must be 'Bearer <token>'" });

        var token = parts[1];

        // Intentar validar firma con la misma key que usa la API
        var handler = new JwtSecurityTokenHandler();
        var keyBytes = Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!);
        var parms = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        try
        {
            var principal = handler.ValidateToken(token, parms, out var validated);
            var jwt = (JwtSecurityToken)validated;
            return Ok(new {
                header = jwt.Header,
                claims = jwt.Claims.Select(c => new { c.Type, c.Value }),
                alg = jwt.Header.Alg,
                kid = jwt.Header.TryGetValue("kid", out var kid) ? kid : null
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Signature/format validation failed", detail = ex.Message, tokenSample = token[..Math.Min(40, token.Length)] + "..." });
        }
    }
}
