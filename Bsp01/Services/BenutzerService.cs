
// Datei: /Services/BenutzerService.cs

using Bsp01.Controllers;
using Bsp01.Data;
using Bsp01.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Bsp01.Services
{
    public class BenutzerService : IBenutzerService
    {

        // DI
        private readonly MeineDbContext _context;
        private readonly IConfiguration _config;

        public BenutzerService(
            MeineDbContext context, 
            IConfiguration config
            )
        {
            _context = context;
            _config = config;
        }


        // --- CRUD --- 
        
        public async Task<IEnumerable<Benutzer>> Get()
        {
            return await _context.Benutzers.ToListAsync();
        }

        public async Task<long> Create(Benutzer benutzer)
        {
            // Setze die Rolle 'admin', wenn es der erste Benutzer ist
            var anzahl = await Count();
            if (anzahl == 0)
            {
                benutzer.Rolle = "admin";
            }

            // Generiere Salt und hashe das Passwort
            benutzer.Salt = GenerateSalt();
            benutzer.Passwort = HashPassword(benutzer.Passwort, benutzer.Salt);

            // Benutzer in der Datenbank anlegen
            _context.Benutzers.Add(benutzer);
            await _context.SaveChangesAsync();
            return benutzer.BenutzerId;
        }

        public async Task Update(Benutzer benutzer)
        {
            // Hole den aktuellen Benutzer aus der DB
            var aktuellerBenutzer = await Get(benutzer.BenutzerId);
            if (aktuellerBenutzer == null)
            {
                throw new InvalidOperationException("Benutzer nicht gefunden.");
            }

            // Salt verwenden oder beibehalten
            benutzer.Salt = aktuellerBenutzer.Salt;

            // Nur hashen, wenn ein neues Passwort übergeben wurde
            if (!string.IsNullOrEmpty(benutzer.Passwort))
            {
                benutzer.Passwort = HashPassword(benutzer.Passwort, benutzer.Salt);
            }
            else
            {
                // Behalte das alte Passwort, wenn kein neues angegeben wurde
                benutzer.Passwort = aktuellerBenutzer.Passwort;
            }

            _context.Entry(benutzer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(long id)
        {
            var benutzer = await _context.Benutzers.FindAsync(id);
            if (benutzer != null)
            {
                _context.Benutzers.Remove(benutzer);
                await _context.SaveChangesAsync();
            }
        }



        // --- REGISTRIEREN --- 

        public async Task<long> Register(RegisterRequest registerRequest)
        {
            var salt = GenerateSalt();
            var neuerBenutzer = new Benutzer
            {
                Name = registerRequest.Name,
                Email = registerRequest.Email,
                Salt = salt,
                Passwort = HashPassword(registerRequest.Passwort, salt),
                //Rolle = registerRequest.Rolle // Beim Registrieren darf das nicht mauel angegeben werden können!
            };

            if (await ExistsByEmail(neuerBenutzer.Email))
            {
                throw new InvalidOperationException("Ein Benutzer mit dieser E-Mail existiert bereits.");
            }

            _context.Benutzers.Add(neuerBenutzer);
            await _context.SaveChangesAsync();
            return neuerBenutzer.BenutzerId;
        }

        public async Task Change(ChangeRequest changeRequest)
        {
            var benutzer = await GetByEmail(changeRequest.Email);
            if (benutzer == null)
            {
                throw new InvalidOperationException("Benutzer nicht gefunden.");
            }

            // Prüfen, ob das alte Passwort korrekt ist
            var hashedAltesPasswort = HashPassword(changeRequest.AltesPasswort, benutzer.Salt);
            if (hashedAltesPasswort != benutzer.Passwort)
            {
                throw new UnauthorizedAccessException("Das alte Passwort ist falsch.");
            }

            // Neues Passwort hashen und speichern
            benutzer.Passwort = HashPassword(changeRequest.NeuesPasswort, benutzer.Salt);
            await Update(benutzer);
        }



        // --- WEITERE METHODEN ---

        public async Task<Benutzer?> Get(long id)
        {
            return await _context.Benutzers.FindAsync(id);
        }

        public async Task<bool> ExistsById(long id)
        {
            return await _context.Benutzers.AnyAsync(b => b.BenutzerId == id);
        }

        public async Task<bool> ExistsByEmail(string email)
        {
            return await _context.Benutzers.AnyAsync(b => b.Email == email);
        }

        public async Task<Benutzer> GetByEmail(string email)
        {
            var benutzer = await _context.Benutzers.FirstOrDefaultAsync(b => b.Email == email);
            if (benutzer == null)
            {
                throw new InvalidOperationException("Benutzer nicht gefunden.");
            }
            return benutzer;
        }

        public async Task<int> Count()
        {
            return await _context.Benutzers.CountAsync();
        }

        public string GenerateSalt()
        {
            var randomBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return BitConverter.ToString(randomBytes).Replace("-", "").ToLower();
        }

        public string HashPassword(string password, string salt)
        {
            var saltedPassword = password + salt;
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public bool VerifyPassword(string password, string salt, string hashedPassword)
        {
            var saltedPassword = password + salt;
            using (var sha256 = SHA256.Create())
            {
                var computedHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                var computedHashString = BitConverter.ToString(computedHash).Replace("-", "").ToLower();
                return computedHashString == hashedPassword;
            }
        }

        public string GenerateJwtToken(Benutzer benutzer)
        {
            var tokenKey = _config["TOKEN_KEY"] ?? "EinLangerGeheimerSchluesselVonMindestens32Zeichen123";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, benutzer.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", benutzer.BenutzerId.ToString()),
                new Claim("email", benutzer.Email),
                new Claim("rolle", benutzer.Rolle) // RoleClaimType = "rolle" muss auch in Program.cs angegeben werden!
                // der Standard-Claim-Typ für Rollen üblicherweise ist "role" oder "roles"
            };

            var token = new JwtSecurityToken(
                issuer: _config["TOKEN_BASE_URL"] ?? "http://localhost",
                audience: _config["TOKEN_BASE_URL"] ?? "http://localhost",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }

}
