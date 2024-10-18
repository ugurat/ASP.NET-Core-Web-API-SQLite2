using Bsp01.Data;
using Bsp01.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Plugins;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bsp01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        // DI
        private readonly IBenutzerService _benutzerService;
        public LoginController(IBenutzerService benutzerService)
        {
            _benutzerService = benutzerService;
        }


        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Passwort))
            {
                return BadRequest(new { message = "Email und Passwort müssen angegeben werden." });
            }

            try
            {
                var benutzer = await _benutzerService.GetByEmail(loginRequest.Email);

                if (benutzer != null && _benutzerService.VerifyPassword(loginRequest.Passwort, benutzer.Salt, benutzer.Passwort))
                {
                    // JWT erstellen
                    var token = _benutzerService.GenerateJwtToken(benutzer);

                    return Ok(new
                    {
                        message = "Login erfolgreich!",
                        id = benutzer.BenutzerId,
                        name = benutzer.Name,
                        email = benutzer.Email,
                        token = token
                    });
                }
                else
                {
                    return Unauthorized(new { message = "Falsche Email oder Passwort." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ein Fehler ist aufgetreten: " + ex.Message });
            }
        }

    }


    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Passwort { get; set; } = string.Empty;
    }

}
