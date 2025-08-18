using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("whoami")]
public class WhoAmIController : ControllerBase
{
    [HttpGet, Authorize]
    public IActionResult Get()
    {
        var id = DreamInCodeApi.Utils.JwtHelper.GetUserId(User);
        return Ok(new {
            userId = id,                                      
            sub = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value,
            nameidentifier = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}
