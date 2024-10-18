
// Datei: /Controllers/BenutzerController.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bsp01.Data;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.AspNetCore.Authorization;
using Bsp01.Interfaces;

namespace Bsp01.Controllers
{

    [Authorize(Roles = "admin")] // JWT-Authentifizierung bei allen Methoden erforderlich
    [Route("api/[controller]")]
    [ApiController]
    public class BenutzersController : ControllerBase
    {

        // Dependency Injection
        private readonly IBenutzerService _benutzerService;
        public BenutzersController(IBenutzerService benutzerService)
        {
            _benutzerService = benutzerService;
        }



        // GET: api/Benutzers
        [HttpGet]
        //[AllowAnonymous] // Registrierung soll ohne Authentifizierung möglich sein
        public async Task<ActionResult<IEnumerable<Benutzer>>> GetBenutzers()
        {
            var benutzerList = await _benutzerService.Get(); // Service-Methode verwenden
            if (benutzerList == null || !benutzerList.Any())
            {
                return NotFound();
            }
            return benutzerList.ToList();
        }

        // GET: api/Benutzers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Benutzer>> GetBenutzer(long id)
        {
            var benutzer = await _benutzerService.Get(id); // Service-Methode verwenden
            if (benutzer == null)
            {
                return NotFound();
            }

            return benutzer;
        }

        // PUT: api/Benutzers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBenutzer(long id, Benutzer benutzer)
        {
            if (id != benutzer.BenutzerId)
            {
                return BadRequest();
            }

            try
            {
                await _benutzerService.Update(benutzer); // Service-Methode verwenden
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _benutzerService.ExistsById(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Benutzers
        [HttpPost]
        //[AllowAnonymous] // Registrierung soll ohne Authentifizierung möglich sein
        public async Task<ActionResult<Benutzer>> PostBenutzer(Benutzer benutzer)
        {
            var benutzerId = await _benutzerService.Create(benutzer); // Service-Methode verwenden

            return CreatedAtAction("GetBenutzer", new { id = benutzerId }, benutzer);
        }

        // DELETE: api/Benutzers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBenutzer(long id)
        {
            var benutzer = await _benutzerService.Get(id); // Service-Methode verwenden
            if (benutzer == null)
            {
                return NotFound();
            }

            await _benutzerService.Delete(id); // Service-Methode verwenden

            return NoContent();
        }

        // Benutzer anhand der E-Mail abrufen
        // POST: api/Benutzers/getByEmail
        [HttpPost("getByEmail")]
        public async Task<Benutzer?> GetByEmail(string email)
        {
            return await _benutzerService.GetByEmail(email); // Service-Methode verwenden
        }

        // Passwortüberprüfung
        [HttpPost("verifyPassword")]
        public bool VerifyPassword(string password, string salt, string hashedPassword)
        {
            return _benutzerService.VerifyPassword(password, salt, hashedPassword); // Service verwenden
        }

    }


}
