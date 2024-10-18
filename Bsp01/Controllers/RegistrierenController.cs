using Bsp01.Data;
using Bsp01.Interfaces;
using Bsp01.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bsp01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrierenController : ControllerBase
    {

        // DI
        private readonly IBenutzerService _benutzerService;
        public RegistrierenController(IBenutzerService benutzerService)
        {
            _benutzerService = benutzerService;
        }

        [HttpPost]
        public async Task<IActionResult> Registrieren([FromBody] RegisterRequest registerRequest)
        {
            if (registerRequest == null || string.IsNullOrEmpty(registerRequest.Name) || string.IsNullOrEmpty(registerRequest.Email) || string.IsNullOrEmpty(registerRequest.Passwort))
            {
                return BadRequest(new { message = "Name, Email und Passwort müssen angegeben werden." });
            }

            // E-Mail Validierung mit Regex
            if (!Regex.IsMatch(registerRequest.Email, "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$"))
            {
                return BadRequest(new { message = "Ungültige E-Mail-Adresse." });
            }

            // Passwort-Längenprüfung
            if (registerRequest.Passwort.Length < 3)
            {
                return BadRequest(new { message = "Passwort muss mindestens 3 Zeichen lang sein." });
            }

            try
            {
                var benutzerId = await _benutzerService.Register(registerRequest); // Service verwenden
                return Ok(new { message = "Registrierung erfolgreich!", id = benutzerId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Fehler bei der Registrierung: " + ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> PasswortAendern([FromBody] ChangeRequest changeRequest)
        {
            if (changeRequest == null || string.IsNullOrEmpty(changeRequest.Email) || string.IsNullOrEmpty(changeRequest.AltesPasswort) || string.IsNullOrEmpty(changeRequest.NeuesPasswort))
            {
                return BadRequest(new { message = "Email, altes Passwort und neues Passwort müssen angegeben werden." });
            }

            // Passwort-Längenprüfung
            if (changeRequest.NeuesPasswort.Length < 3)
            {
                return BadRequest(new { message = "Neues Passwort muss mindestens 3 Zeichen lang sein." });
            }

            try
            {
                await _benutzerService.Change(changeRequest); 
                return Ok(new { message = "Passwort erfolgreich geändert!" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Das alte Passwort ist falsch." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Fehler beim Ändern des Passworts: " + ex.Message });
            }

        }

        /*
        private string HashPassword(string password, string salt)
        {
            // Passwort mit Salt hashen
            var saltedPassword = password + salt;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(saltedPassword));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private string GenerateSalt()
        {
            // 16 zufällige Bytes generieren und in einen Hex-String umwandeln
            var randomBytes = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return BitConverter.ToString(randomBytes).Replace("-", "").ToLower();
        }
        */

    }

    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Passwort { get; set; } = string.Empty;
        //public string Rolle { get; set; } = "user"; // Beim Registrieren darf das nicht mauel angegeben werden können!
    }

    public class ChangeRequest
    {
        public string Email { get; set; } = string.Empty;
        public string AltesPasswort { get; set; } = string.Empty;
        public string NeuesPasswort { get; set; } = string.Empty;
    }


}
