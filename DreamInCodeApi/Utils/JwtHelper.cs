using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace DreamInCodeApi.Utils;

public static class JwtHelper
{
    public static string CreateToken(int userId, IConfiguration cfg)
    {
        var issuer   = cfg["Jwt:Issuer"];
        var audience = cfg["Jwt:Audience"];
        var key      = cfg["Jwt:Key"]!;                  
        var keyBytes = Encoding.UTF8.GetBytes(key);      

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static int? GetUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirstValue("nameid");

        return int.TryParse(raw, out var id) ? id : null;
    }
}